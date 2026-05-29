using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.Centralize.Application.Abstractions;

namespace Anemoi.Centralize.Infrastructure.Services;

public sealed class DockerService : IDockerService
{
    private const string DefaultSocketPath = "/var/run/docker.sock";

    private HttpClient CreateDockerHttpClient()
    {
        var socketPath = DefaultSocketPath;

        if (!File.Exists(socketPath))
        {
            // Try standard macOS host socket path if not found in default location (for host runs)
            var userSocketPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".docker/run/docker.sock"
            );
            if (File.Exists(userSocketPath))
            {
                socketPath = userSocketPath;
            }
        }

        var handler = new SocketsHttpHandler
        {
            ConnectCallback = async (context, cancellationToken) =>
            {
                var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                var endpoint = new UnixDomainSocketEndPoint(socketPath);
                await socket.ConnectAsync(endpoint, cancellationToken);
                return new NetworkStream(socket, ownsSocket: true);
            }
        };

        return new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost"),
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    public async Task<ContainerStatusDto> GetContainerStatusAsync(
        string containerName,
        string displayName,
        string defaultUrl,
        string description,
        CancellationToken cancellationToken
    )
    {
        try
        {
            using var client = CreateDockerHttpClient();
            var response = await client.GetAsync($"/containers/{containerName}/json", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new ContainerStatusDto(
                    containerName,
                    displayName,
                    "stopped",
                    defaultUrl,
                    description,
                    DateTime.UtcNow
                );
            }

            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);

            using var doc = JsonDocument.Parse(jsonString);
            var state = doc.RootElement.GetProperty("State");
            var running = state.GetProperty("Running").GetBoolean();
            var status = state.GetProperty("Status").GetString() ?? "stopped";

            var finishedAtStr = state.GetProperty("FinishedAt").GetString();
            var startedAtStr = state.GetProperty("StartedAt").GetString();

            var lastUpdated = DateTime.UtcNow;
            if (running && DateTime.TryParse(startedAtStr, out var startedAt))
            {
                lastUpdated = startedAt.ToUniversalTime();
            }
            else if (DateTime.TryParse(finishedAtStr, out var finishedAt))
            {
                lastUpdated = finishedAt.ToUniversalTime();
            }

            // Standardize status: running, stopped, starting, stopping, error
            var finalStatus = "stopped";
            if (status == "running")
                finalStatus = "running";
            else if (status == "restarting" || status == "paused")
                finalStatus = "starting";
            else if (status == "exited" || status == "dead")
                finalStatus = "stopped";
            else if (status == "removing")
                finalStatus = "stopping";

            return new ContainerStatusDto(
                containerName,
                displayName,
                finalStatus,
                defaultUrl,
                description,
                lastUpdated
            );
        }
        catch (Exception ex)
        {
            // If docker daemon is unreachable
            return new ContainerStatusDto(
                containerName,
                displayName,
                "stopped",
                defaultUrl,
                $"{description} (Docker daemon unreachable)",
                DateTime.UtcNow
            );
        }
    }

    public async Task<bool> StartContainerAsync(string containerName, CancellationToken cancellationToken)
    {
        try
        {
            using var client = CreateDockerHttpClient();
            var response = await client.PostAsync($"/containers/{containerName}/start", null, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> StopContainerAsync(string containerName, CancellationToken cancellationToken)
    {
        try
        {
            using var client = CreateDockerHttpClient();
            var response = await client.PostAsync($"/containers/{containerName}/stop?t=5", null, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
