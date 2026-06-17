using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Workflow;

public sealed class WorkflowDefinitionStep : Entity<WorkflowDefinitionStepId>
{
    public WorkflowDefinitionId WorkflowDefinitionId { get; private set; }
    public int Sequence { get; private set; }
    public string ApproverType { get; private set; }
    public string? ApproverValue { get; private set; }
    public bool IsRequired { get; private set; }

    private WorkflowDefinitionStep() { }

    public static WorkflowDefinitionStep Create(
        WorkflowDefinitionStepId id,
        WorkflowDefinitionId workflowDefinitionId,
        int sequence,
        string approverType,
        string? approverValue,
        bool isRequired)
    {
        return new WorkflowDefinitionStep
        {
            Id = id,
            WorkflowDefinitionId = workflowDefinitionId,
            Sequence = sequence,
            ApproverType = approverType,
            ApproverValue = approverValue,
            IsRequired = isRequired
        };
    }
}
