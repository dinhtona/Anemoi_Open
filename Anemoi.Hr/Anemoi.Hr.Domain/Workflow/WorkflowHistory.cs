using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Workflow;

public sealed class WorkflowHistory : Entity<WorkflowHistoryId>
{
    public WorkflowInstanceId WorkflowInstanceId { get; private set; }
    public string Action { get; private set; }
    public string PerformedBy { get; private set; }
    public string? Comment { get; private set; }
    public DateTime PerformedAt { get; private set; }

    private WorkflowHistory() { }

    internal static WorkflowHistory Create(
        WorkflowHistoryId id,
        WorkflowInstanceId workflowInstanceId,
        string action,
        string performedBy,
        string? comment)
    {
        return new WorkflowHistory
        {
            Id = id,
            WorkflowInstanceId = workflowInstanceId,
            Action = action,
            PerformedBy = performedBy,
            Comment = comment,
            PerformedAt = DateTime.UtcNow
        };
    }
}
