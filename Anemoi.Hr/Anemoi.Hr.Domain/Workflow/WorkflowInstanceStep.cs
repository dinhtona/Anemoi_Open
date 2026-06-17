using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Workflow;

public sealed class WorkflowInstanceStep : Entity<WorkflowInstanceStepId>
{
    public WorkflowInstanceId WorkflowInstanceId { get; private set; }
    public int Sequence { get; private set; }
    public string ApproverTypeSnapshot { get; private set; }
    public string? ApproverValueSnapshot { get; private set; }
    public string? ApproverUserId { get; private set; }
    public string Status { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? RejectedAt { get; private set; }
    public string? Comment { get; private set; }

    private WorkflowInstanceStep() { }

    public static WorkflowInstanceStep Create(
        WorkflowInstanceStepId id,
        WorkflowInstanceId workflowInstanceId,
        int sequence,
        string approverTypeSnapshot,
        string? approverValueSnapshot,
        string? approverUserId)
    {
        return new WorkflowInstanceStep
        {
            Id = id,
            WorkflowInstanceId = workflowInstanceId,
            Sequence = sequence,
            ApproverTypeSnapshot = approverTypeSnapshot,
            ApproverValueSnapshot = approverValueSnapshot,
            ApproverUserId = approverUserId,
            Status = WorkflowStepStatusCode.Pending
        };
    }

    internal void Approve(string? comment)
    {
        Status = WorkflowStepStatusCode.Approved;
        ApprovedAt = DateTime.UtcNow;
        Comment = comment;
    }

    internal void Reject(string? comment)
    {
        Status = WorkflowStepStatusCode.Rejected;
        RejectedAt = DateTime.UtcNow;
        Comment = comment;
    }

    internal void Cancel()
    {
        Status = WorkflowStepStatusCode.Cancelled;
    }
}
