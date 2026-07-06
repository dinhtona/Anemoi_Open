using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Domain.Workflow;
using MediatR;

namespace Anemoi.Hr.Application.Cqrs.Events;

public sealed class WorkflowInstanceApprovedHandler(
    IEnumerable<IWorkflowTargetStatusUpdater> updaters)
    : INotificationHandler<WorkflowInstanceApprovedDomainEvent>
{
    public async Task Handle(WorkflowInstanceApprovedDomainEvent evt, CancellationToken cancellationToken)
    {
        var updater = updaters.FirstOrDefault(u => u.CanHandle(evt.EntityType));
        if (updater is null) return;
        await updater.MarkApprovedAsync(evt.EntityId, evt.PerformedBy, cancellationToken);
    }
}
