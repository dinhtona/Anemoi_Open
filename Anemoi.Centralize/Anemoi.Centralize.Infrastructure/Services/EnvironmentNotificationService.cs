using System;
using System.Threading.Tasks;
using Anemoi.Centralize.Application.Abstractions;
using Anemoi.Contract.Notification.Commands.NotificationCommands.CreateNotification;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Anemoi.Centralize.Infrastructure.Services;

public sealed class EnvironmentNotificationService(
    IConnectedUsersRegistry registry,
    ISender sender,
    ILogger<EnvironmentNotificationService> logger) : IEnvironmentNotificationService
{
    public async Task NotifyEnvironmentActivityAsync(string serviceName, string actionDetails)
    {
        var activeUserIds = registry.GetActiveUserIds();
        if (activeUserIds.Count == 0)
        {
            logger.LogInformation("No active users connected via SignalR. Skipping environment notification for {Service}.", serviceName);
            return;
        }

        foreach (var userId in activeUserIds)
        {
            try
            {
                var command = new CreateNotificationCommand(
                    UserId: userId,
                    Title: $"Environment Call: {serviceName}",
                    Content: actionDetails,
                    Category: "Environment"
                );

                await sender.Send(command);
                logger.LogInformation("Sent environment notification to user {UserId}: {Details}", userId, actionDetails);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send environment notification to user {UserId}", userId);
            }
        }
    }
}
