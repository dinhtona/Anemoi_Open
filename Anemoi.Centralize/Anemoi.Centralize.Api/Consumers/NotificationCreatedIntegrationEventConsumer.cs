using System;
using System.Threading.Tasks;
using Anemoi.Centralize.Api.Hubs;
using Anemoi.Contract.Notification.Events;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace Anemoi.Centralize.Api.Consumers;

public sealed class NotificationCreatedIntegrationEventConsumer(
    IHubContext<NotificationHub> hubContext,
    Serilog.ILogger logger)
    : IConsumer<NotificationCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<NotificationCreatedIntegrationEvent> context)
    {
        var @event = context.Message;
        try
        {
            // Send the notification to the specific user via their User ID
            await hubContext.Clients.User(@event.UserId).SendAsync("ReceiveNotification", @event);

            logger.Information("Real-time notification pushed via SignalR to User: {UserId} for Notification: {NotificationId}", @event.UserId, @event.Id);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to push real-time notification to User: {UserId} via SignalR", @event.UserId);
        }
    }
}
