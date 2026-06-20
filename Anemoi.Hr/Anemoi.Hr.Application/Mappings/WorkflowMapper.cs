using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Mappings;

public sealed class WorkflowMapper
{
    public WorkflowDefinitionResponse ToResponse(WorkflowDefinition def)
    {
        if (def is null) return null;
        return new WorkflowDefinitionResponse(
            Id: def.Id.Value.ToString(),
            Code: def.Code,
            Name: def.Name,
            Description: def.Description,
            WorkflowTypeCode: def.WorkflowTypeCode,
            TargetEntityType: def.TargetEntityType,
            Version: def.Version,
            IsActive: def.IsActive,
            Steps: def.Steps.Select(ToStepResponse).ToList(),
            CreatedAt: def.CreatedAt,
            UpdatedAt: def.UpdatedAt);
    }

    public WorkflowDefinitionStepResponse ToStepResponse(WorkflowDefinitionStep step)
    {
        if (step is null) return null;
        return new WorkflowDefinitionStepResponse(
            Id: step.Id.Value.ToString(),
            Sequence: step.Sequence,
            ApproverType: step.ApproverType,
            ApproverValue: step.ApproverValue,
            IsRequired: step.IsRequired);
    }

    public WorkflowInstanceResponse ToResponse(WorkflowInstance instance, string definitionName = null)
    {
        if (instance is null) return null;
        return new WorkflowInstanceResponse(
            Id: instance.Id.Value.ToString(),
            WorkflowDefinitionId: instance.WorkflowDefinitionId?.Value.ToString(),
            WorkflowDefinitionName: definitionName,
            EntityType: instance.EntityType,
            EntityId: instance.EntityId,
            CurrentStep: instance.CurrentStep,
            Status: instance.Status,
            StartedBy: instance.StartedBy,
            RequesterEmployeeId: instance.RequesterEmployeeId.Value.ToString(),
            RequesterUserId: instance.RequesterUserId.Value.ToString(),
            StartedAt: instance.StartedAt,
            CompletedAt: instance.CompletedAt,
            Steps: instance.Steps.Select(ToStepResponse).ToList(),
            Histories: instance.Histories.Select(ToHistoryResponse).ToList());
    }

    public WorkflowInstanceStepResponse ToStepResponse(WorkflowInstanceStep step)
    {
        if (step is null) return null;
        return new WorkflowInstanceStepResponse(
            Id: step.Id.Value.ToString(),
            Sequence: step.Sequence,
            ApproverTypeSnapshot: step.ApproverTypeSnapshot,
            ApproverValueSnapshot: step.ApproverValueSnapshot,
            ApproverUserId: step.ApproverUserId,
            Status: step.Status,
            ApprovedAt: step.ApprovedAt,
            RejectedAt: step.RejectedAt,
            Comment: step.Comment);
    }

    public WorkflowHistoryResponse ToHistoryResponse(WorkflowHistory history)
    {
        if (history is null) return null;
        return new WorkflowHistoryResponse(
            Id: history.Id.Value.ToString(),
            WorkflowInstanceId: history.WorkflowInstanceId.Value.ToString(),
            Action: history.Action,
            PerformedBy: history.PerformedBy,
            Comment: history.Comment,
            PerformedAt: history.PerformedAt);
    }

    public IReadOnlyCollection<WorkflowDefinitionResponse> ToResponses(IEnumerable<WorkflowDefinition> defs)
    {
        return defs?.Select(ToResponse).ToList() ?? [];
    }

    public IReadOnlyCollection<WorkflowInstanceResponse> ToResponses(IEnumerable<WorkflowInstance> instances,
        IDictionary<Guid, string> definitionNames = null)
    {
        if (instances is null) return [];
        return instances.Select(i =>
        {
            var name = definitionNames is not null
                && i.WorkflowDefinitionId is not null
                && definitionNames.TryGetValue(i.WorkflowDefinitionId.Value, out var n) ? n : null;
            return ToResponse(i, name);
        }).ToList();
    }
}
