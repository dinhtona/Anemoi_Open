using System;
using System.Threading.Tasks;
using Anemoi.Centralize.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Resources;
using Anemoi.Contract.Notification.Commands.NotificationCommands.CreateNotification;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;

namespace Anemoi.Centralize.Infrastructure.Services;

public sealed class EnvironmentNotificationService(
    IConnectedUsersRegistry registry,
    ISender sender,
    IStringLocalizer<SharedResource> localizer,
    ILogger<EnvironmentNotificationService> logger) : IEnvironmentNotificationService
{
    public async Task NotifyEnvironmentActivityAsync(
        string serviceNameResourceKey,
        string actionResourceKey,
        params object[] arguments)
    {
        var serviceName = localizer[serviceNameResourceKey].Value;
        var actionDetails = localizer[actionResourceKey, arguments].Value;
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
                    Title: localizer["EnvironmentActivityTitle", serviceName].Value,
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
