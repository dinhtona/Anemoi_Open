using Anemoi.Contract.Hr;
using Anemoi.Hr.Domain.Workflow;
using MassTransit;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Events;

public sealed class WorkflowApprovedIntegrationEventPublisher(
    IPublishEndpoint publishEndpoint)
    : INotificationHandler<WorkflowInstanceApprovedDomainEvent>
{
    public async Task Handle(WorkflowInstanceApprovedDomainEvent evt, CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(new WorkflowApprovedIntegrationEvent(
            evt.WorkflowInstanceId.Value,
            evt.EntityType,
            evt.EntityId,
            evt.PerformedBy,
            null,
            DateTime.UtcNow,
            true
        ), cancellationToken);
    }
}
