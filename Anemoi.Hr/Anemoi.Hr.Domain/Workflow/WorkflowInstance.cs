using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Workflow;

public sealed class WorkflowInstance : Entity<WorkflowInstanceId>
{
    private readonly List<WorkflowInstanceStep> _steps = [];
    private readonly List<WorkflowHistory> _histories = [];

    public WorkflowDefinitionId WorkflowDefinitionId { get; private set; }
    public string EntityType { get; private set; }
    public string EntityId { get; private set; }
    public int CurrentStep { get; private set; }
    public string Status { get; private set; }
    public string StartedBy { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public IReadOnlyCollection<WorkflowInstanceStep> Steps => _steps.AsReadOnly();
    public IReadOnlyCollection<WorkflowHistory> Histories => _histories.AsReadOnly();

    private WorkflowInstance() { }

    private WorkflowInstance(
        WorkflowInstanceId id,
        WorkflowDefinitionId workflowDefinitionId,
        string entityType,
        string entityId,
        string startedBy,
        List<WorkflowInstanceStep> steps)
    {
        Id = id;
        WorkflowDefinitionId = workflowDefinitionId;
        EntityType = entityType;
        EntityId = entityId;
        CurrentStep = steps.Count > 0 ? steps.Min(s => s.Sequence) : 0;
        Status = WorkflowStatusCode.Pending;
        StartedBy = startedBy;
        StartedAt = DateTime.UtcNow;
        _steps = steps;
    }

    public static WorkflowInstance Start(
        WorkflowInstanceId id,
        WorkflowDefinitionId workflowDefinitionId,
        string entityType,
        string entityId,
        string startedBy,
        List<WorkflowInstanceStep> steps)
    {
        return new WorkflowInstance(id, workflowDefinitionId, entityType, entityId, startedBy, steps);
    }

    public WorkflowHistory Approve(string performedBy, string? comment)
    {
        EnsurePending();
        var step = GetCurrentStepEntity();
        step.Approve(comment);
        var history = WorkflowHistory.Create(
            new WorkflowHistoryId(Guid.NewGuid()), Id,
            "Approve", performedBy, comment);
        AddHistory(history);

        var nextSequence = step.Sequence + 1;
        var nextStep = _steps.FirstOrDefault(s => s.Sequence == nextSequence);
        if (nextStep is null || nextStep.Status != WorkflowStepStatusCode.Pending)
        {
            Status = WorkflowStatusCode.Approved;
            CompletedAt = DateTime.UtcNow;
        }
        else
        {
            CurrentStep = nextSequence;
        }

        return history;
    }

    public WorkflowHistory Reject(string performedBy, string? comment)
    {
        EnsurePending();
        var step = GetCurrentStepEntity();
        step.Reject(comment);
        Status = WorkflowStatusCode.Rejected;
        CompletedAt = DateTime.UtcNow;
        var history = WorkflowHistory.Create(
            new WorkflowHistoryId(Guid.NewGuid()), Id,
            "Reject", performedBy, comment);
        AddHistory(history);
        return history;
    }

    public WorkflowHistory Cancel(string performedBy)
    {
        if (Status is WorkflowStatusCode.Approved or WorkflowStatusCode.Rejected or WorkflowStatusCode.Cancelled)
            throw new InvalidOperationException($"Cannot cancel workflow in status '{Status}'.");
        foreach (var step in _steps.Where(s => s.Status == WorkflowStepStatusCode.Pending))
            step.Cancel();
        Status = WorkflowStatusCode.Cancelled;
        CompletedAt = DateTime.UtcNow;
        var history = WorkflowHistory.Create(
            new WorkflowHistoryId(Guid.NewGuid()), Id,
            "Cancel", performedBy, null);
        AddHistory(history);
        return history;
    }

    public WorkflowHistory ReturnForRevision(string performedBy, string? comment)
    {
        EnsurePending();
        foreach (var step in _steps.Where(s => s.Status == WorkflowStepStatusCode.Pending))
            step.Cancel();
        Status = WorkflowStatusCode.Returned;
        CompletedAt = DateTime.UtcNow;
        var history = WorkflowHistory.Create(
            new WorkflowHistoryId(Guid.NewGuid()), Id,
            "ReturnForRevision", performedBy, comment);
        AddHistory(history);
        return history;
    }

    public bool IsCurrentStepApprover(string userId, Func<string, bool> hasRole, Func<string, bool> hasPermission)
    {
        var step = GetCurrentStepEntity();
        return step.ApproverTypeSnapshot switch
        {
            ApproverType.SpecificUser => step.ApproverValueSnapshot == userId,
            ApproverType.DirectManager => step.ApproverUserId == userId,
            ApproverType.Role => step.ApproverValueSnapshot != null && hasRole(step.ApproverValueSnapshot),
            ApproverType.Permission => step.ApproverValueSnapshot != null && hasPermission(step.ApproverValueSnapshot),
            _ => false
        };
    }

    private void EnsurePending()
    {
        if (Status != WorkflowStatusCode.Pending)
            throw new InvalidOperationException($"Workflow is in '{Status}' status, expected 'Pending'.");
    }

    private WorkflowInstanceStep GetCurrentStepEntity()
    {
        return _steps.FirstOrDefault(s => s.Sequence == CurrentStep)
            ?? throw new InvalidOperationException($"No step found at sequence {CurrentStep}.");
    }

    private void AddHistory(WorkflowHistory history)
    {
        _histories.Add(history);
    }
}
