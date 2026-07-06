using Anemoi.Contract.Hr;
using Anemoi.Hr.Domain.Workflow;
using MassTransit;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Events;

public sealed class WorkflowRejectedIntegrationEventPublisher(
    IPublishEndpoint publishEndpoint)
    : INotificationHandler<WorkflowInstanceRejectedDomainEvent>
{
    public async Task Handle(WorkflowInstanceRejectedDomainEvent evt, CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(new WorkflowRejectedIntegrationEvent(
            evt.WorkflowInstanceId.Value,
            evt.EntityType,
            evt.EntityId,
            evt.PerformedBy,
            evt.Comment,
            DateTime.UtcNow,
            true
        ), cancellationToken);
    }
}
