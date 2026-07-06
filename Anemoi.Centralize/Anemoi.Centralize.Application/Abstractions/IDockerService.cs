using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Centralize.Application.Abstractions;

public record ContainerStatusDto(
    string Id,
    string Name,
    string Status,
    string Url,
    string Description,
    DateTime LastUpdated
);

public interface IDockerService
{
    Task<ContainerStatusDto> GetContainerStatusAsync(
        string containerName,
        string displayName,
        string defaultUrl,
        string description,
        CancellationToken cancellationToken
    );
    Task<bool> StartContainerAsync(string containerName, CancellationToken cancellationToken);
    Task<bool> StopContainerAsync(string containerName, CancellationToken cancellationToken);
}
