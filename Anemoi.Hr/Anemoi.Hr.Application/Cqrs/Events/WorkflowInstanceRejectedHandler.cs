using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Domain.Workflow;
using MediatR;
using Serilog;

namespace Anemoi.Hr.Application.Cqrs.Events;

public sealed class WorkflowInstanceRejectedHandler(
    IEnumerable<IWorkflowTargetStatusUpdater> updaters)
    : INotificationHandler<WorkflowInstanceRejectedDomainEvent>
{
    public async Task Handle(WorkflowInstanceRejectedDomainEvent evt, CancellationToken cancellationToken)
    {
        var updater = updaters.FirstOrDefault(u => u.CanHandle(evt.EntityType));
        if (updater is null) return;
        var result = await updater.MarkRejectedAsync(evt.EntityId, evt.PerformedBy, evt.Comment, cancellationToken);
        if (result.IsT1)
            Log.Warning("Failed to mark {EntityType} {EntityId} as rejected: {ErrorCode}",
                evt.EntityType, evt.EntityId, result.AsT1.Code);
    }
}
