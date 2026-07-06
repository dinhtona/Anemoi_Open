using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Contract.Identity.Events;
using MassTransit;
using Serilog;

namespace Anemoi.Centralize.Api.Consumers;

public sealed class RoleGroupPermissionChangedIntegrationEventConsumer(
    IPermissionResolver permissionResolver,
    Serilog.ILogger logger)
    : IConsumer<RoleGroupPermissionChangedIntegrationEvent>
{
    public Task Consume(ConsumeContext<RoleGroupPermissionChangedIntegrationEvent> context)
    {
        var @event = context.Message;
        permissionResolver.InvalidateRoleGroup(@event.RoleGroupCode);
        logger.Information("Invalidated permission cache for role group {Code} at {ChangedAt}",
            @event.RoleGroupCode, @event.ChangedAt);
        return Task.CompletedTask;
    }
}
