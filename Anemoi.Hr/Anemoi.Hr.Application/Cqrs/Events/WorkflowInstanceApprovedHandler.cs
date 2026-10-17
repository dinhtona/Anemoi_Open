using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Domain.Workflow;
using MediatR;
using Serilog;

namespace Anemoi.Hr.Application.Cqrs.Events;

public sealed class WorkflowInstanceApprovedHandler(
    IEnumerable<IWorkflowTargetStatusUpdater> updaters)
    : INotificationHandler<WorkflowInstanceApprovedDomainEvent>
{
    public async Task Handle(WorkflowInstanceApprovedDomainEvent evt, CancellationToken cancellationToken)
    {
        var updater = updaters.FirstOrDefault(u => u.CanHandle(evt.EntityType));
        if (updater is null) return;
        var result = await updater.MarkApprovedAsync(evt.EntityId, evt.PerformedBy, cancellationToken);
        if (result.IsT1)
            Log.Warning("Failed to mark {EntityType} {EntityId} as approved: {ErrorCode}",
                evt.EntityType, evt.EntityId, result.AsT1.Code);
    }
}
