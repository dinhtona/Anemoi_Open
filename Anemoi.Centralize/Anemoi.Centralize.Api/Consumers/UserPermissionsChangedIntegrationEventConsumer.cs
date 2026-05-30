using System.Threading.Tasks;
using Anemoi.Centralize.Api.Hubs;
using Anemoi.Contract.Identity.Events;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace Anemoi.Centralize.Api.Consumers;

public sealed class UserPermissionsChangedIntegrationEventConsumer(
    IHubContext<NotificationHub> hubContext,
    Serilog.ILogger logger)
    : IConsumer<UserPermissionsChangedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UserPermissionsChangedIntegrationEvent> context)
    {
        var @event = context.Message;
        await hubContext.Clients.User(@event.UserId.ToString())
            .SendAsync("UserPermissionsChanged", cancellationToken: context.CancellationToken);
        logger.Information("Published permission refresh for user {UserId} at {ChangedAt}",
            @event.UserId, @event.ChangedAt);
    }
}
