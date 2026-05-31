#nullable enable
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.Centralize.Application.Abstractions;
using Anemoi.Centralize.Application.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Anemoi.Centralize.Api.Services;

public sealed class Smtp4DevMonitoringWorker(
    IServiceProvider serviceProvider,
    DevEnvironmentsSetting settings,
    ILogger<Smtp4DevMonitoringWorker> logger) : BackgroundService
{
    private readonly HashSet<string> _seenMessageIds = new();
    private readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(3) };
    private bool _initialized = false;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Smtp4Dev Monitoring Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PollMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogTrace(ex, "Error occurred during Smtp4Dev polling (SMTP server might be offline).");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task PollMessagesAsync(CancellationToken cancellationToken)
    {
        // Try multiple endpoints depending on execution context (in-docker service name vs localhost)
        var endpoints = new[]
        {
            "http://smtp4dev/api/messages",
            "http://smtp4dev_server/api/messages",
            settings.MailWebUiUrl?.TrimEnd('/') + "/api/messages"
        };

        List<Smtp4DevMessageDto>? messages = null;
        foreach (var url in endpoints)
        {
            if (string.IsNullOrEmpty(url)) continue;
            try
            {
                messages = await _httpClient.GetFromJsonAsync<List<Smtp4DevMessageDto>>(url, cancellationToken);
                if (messages != null) break;
            }
            catch
            {
                // Try next endpoint
            }
        }

        if (messages == null) return;

        if (!_initialized)
        {
            // Record existing messages on startup to avoid spamming historical emails
            foreach (var msg in messages)
            {
                if (!string.IsNullOrEmpty(msg.Id))
                {
                    _seenMessageIds.Add(msg.Id);
                }
            }
            _initialized = true;
            logger.LogInformation("Smtp4Dev Monitoring Worker initialized with {Count} historical messages.", _seenMessageIds.Count);
            return;
        }

        foreach (var msg in messages)
        {
            if (string.IsNullOrEmpty(msg.Id) || _seenMessageIds.Contains(msg.Id)) continue;

            _seenMessageIds.Add(msg.Id);

            // Send notification
            using var scope = serviceProvider.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<IEnvironmentNotificationService>();
            
            await notificationService.NotifyEnvironmentActivityAsync(
                "EnvironmentServiceMailTest",
                "EnvironmentMailReceived",
                msg.From ?? "",
                msg.To ?? "",
                msg.Subject ?? "");
        }
    }

    private sealed class Smtp4DevMessageDto
    {
        public string? Id { get; set; }
        public string? From { get; set; }
        public string? To { get; set; }
        public string? Subject { get; set; }
    }
}
