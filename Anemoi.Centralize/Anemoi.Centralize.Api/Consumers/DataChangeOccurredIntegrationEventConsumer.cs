using System;
using System.Threading.Tasks;
using Anemoi.Centralize.Api.Hubs;
using Anemoi.Contract.Notification.Constants;
using Anemoi.Contract.Notification.Events;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace Anemoi.Centralize.Api.Consumers;

public sealed class DataChangeOccurredIntegrationEventConsumer(
    IHubContext<NotificationHub> hubContext,
    Serilog.ILogger logger)
    : IConsumer<DataChangeOccurredIntegrationEvent>
{
    public async Task Consume(ConsumeContext<DataChangeOccurredIntegrationEvent> context)
    {
        var @event = context.Message;
        try
        {
            if (string.IsNullOrEmpty(@event.WorkspaceId))
            {
                // Security Check: If WorkspaceId is null, and sensitivity is not Low, do NOT broadcast globally.
                if (!string.Equals(@event.Sensitivity, NotificationConstants.DataSensitivity.Low, StringComparison.OrdinalIgnoreCase))
                {
                    logger.Warning("Skipping global broadcast of sensitive data change event. Resource: {Resource}, Action: {Action}, Sensitivity: {Sensitivity}, EntityId: {EntityId}",
                        @event.Resource, @event.Action, @event.Sensitivity, @event.EntityId);
                    return;
                }

                // Global broadcast for low-sensitivity global master data changes
                await hubContext.Clients.All.SendAsync(
                    NotificationConstants.SignalRMethods.ReceiveDataChange,
                    @event,
                    context.CancellationToken);

                logger.Information("Pushed global low-sensitivity data change event to all clients. Resource: {Resource}", @event.Resource);
            }
            else
            {
                // Scoped broadcast to the specific workspace group
                var groupName = $"Workspace-{@event.WorkspaceId}";
                await hubContext.Clients.Group(groupName).SendAsync(
                    NotificationConstants.SignalRMethods.ReceiveDataChange,
                    @event,
                    context.CancellationToken);

                logger.Information("Pushed data change event to group {GroupName}. Resource: {Resource}, Action: {Action}", 
                    groupName, @event.Resource, @event.Action);
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to push real-time data change event via SignalR. Resource: {Resource}, Action: {Action}", 
                @event.Resource, @event.Action);
        }
    }
}
