using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Domain.Workflow;
using MediatR;

namespace Anemoi.Hr.Application.Cqrs.Events;

public sealed class WorkflowInstanceRejectedHandler(
    IEnumerable<IWorkflowTargetStatusUpdater> updaters)
    : INotificationHandler<WorkflowInstanceRejectedDomainEvent>
{
    public async Task Handle(WorkflowInstanceRejectedDomainEvent evt, CancellationToken cancellationToken)
    {
        var updater = updaters.FirstOrDefault(u => u.CanHandle(evt.EntityType));
        if (updater is null) return;
        await updater.MarkRejectedAsync(evt.EntityId, evt.PerformedBy, evt.Comment, cancellationToken);
    }
}
