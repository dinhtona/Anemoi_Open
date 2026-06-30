using Anemoi.Contract.Hr.Events;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Anemoi.BuildingBlock.Application.Abstractions;

namespace Anemoi.Hr.Application.Cqrs.Events;

public sealed class WorkflowStepApprovedHandler(
    ISqlRepository<WorkflowInstance> instanceRepository,
    IPublishEndpoint publishEndpoint)
    : INotificationHandler<WorkflowStepApprovedDomainEvent>
{
    public async Task Handle(WorkflowStepApprovedDomainEvent evt, CancellationToken cancellationToken)
    {
        var instance = await instanceRepository.GetQueryable()
            .Include(x => x.Steps)
            .FirstOrDefaultAsync(x => x.Id == evt.WorkflowInstanceId, cancellationToken);

        if (instance is null) return;

        var currentStep = instance.Steps.FirstOrDefault(s => s.Sequence == instance.CurrentStep);
        if (currentStep?.ApproverEmployeeId is null) return;

        var nextApproverEmployeeId = currentStep.ApproverEmployeeId.Value.ToString();

        if (evt.EntityType == WorkflowConstants.TargetEntityTypes.LeaveRequest)
        {
            await publishEndpoint.Publish(new LeaveRequestSubmittedIntegrationEvent(
                evt.EntityId,
                instance.RequesterEmployeeId.Value.ToString(),
                null,
                nextApproverEmployeeId), cancellationToken);
        }
        else if (evt.EntityType == WorkflowConstants.TargetEntityTypes.OvertimeRequest)
        {
            await publishEndpoint.Publish(new OvertimeRequestCreatedIntegrationEvent(
                evt.EntityId,
                instance.RequesterEmployeeId.Value.ToString(),
                nextApproverEmployeeId), cancellationToken);
        }
    }
}
