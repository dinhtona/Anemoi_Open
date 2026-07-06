using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Onboarding;

public sealed class OnboardingTask : ValueObject
{
    public OnboardingTaskId Id { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public string AssigneeType { get; private set; }
    public string? AssigneeRoleCode { get; private set; }
    public string? AssignedUserId { get; private set; }
    public string? AssignedUserDisplayNameSnapshot { get; private set; }
    public DateTime? AssignedAt { get; private set; }
    public string? AssignedBy { get; private set; }
    public DateTime? ReassignedAt { get; private set; }
    public string? ReassignedBy { get; private set; }
    public DateTime DueDate { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsRequired { get; private set; }
    public string Status { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? CompletedBy { get; private set; }
    public string? CompletedNotes { get; private set; }
    public DateTime? SkippedAt { get; private set; }
    public string? SkippedBy { get; private set; }
    public DateTime? ReopenedAt { get; private set; }
    public string? ReopenedBy { get; private set; }
    public string? ReopenedReason { get; private set; }

    private OnboardingTask() { }

    private OnboardingTask(
        OnboardingTaskId id,
        string title,
        string? description,
        string assigneeType,
        string? assigneeRoleCode,
        string? assignedUserId,
        string? assignedUserDisplayNameSnapshot,
        string assignedBy,
        DateTime dueDate,
        int sortOrder,
        bool isRequired)
    {
        Id = id;
        Title = title;
        Description = description;
        AssigneeType = assigneeType;
        AssigneeRoleCode = assigneeRoleCode;
        AssignedUserId = assignedUserId;
        AssignedUserDisplayNameSnapshot = assignedUserDisplayNameSnapshot;
        AssignedAt = DateTime.UtcNow;
        AssignedBy = assignedBy;
        DueDate = dueDate;
        SortOrder = sortOrder;
        IsRequired = isRequired;
        Status = OnboardingTaskStatusCode.Pending;
    }

    public static OnboardingTask Create(
        OnboardingTaskId id,
        string title,
        string? description,
        string? assigneeRoleCode,
        string? assignedUserId,
        string? assignedUserDisplayNameSnapshot,
        string assignedBy,
        DateTime dueDate,
        int sortOrder,
        bool isRequired = true)
    {
        return new OnboardingTask(
            id, title, description, "Role", assigneeRoleCode,
            assignedUserId, assignedUserDisplayNameSnapshot,
            assignedBy, dueDate, sortOrder, isRequired);
    }

    public bool Complete(string completedBy, string? notes = null)
    {
        if (Status != OnboardingTaskStatusCode.Pending) return false;
        Status = OnboardingTaskStatusCode.Completed;
        CompletedAt = DateTime.UtcNow;
        CompletedBy = completedBy;
        CompletedNotes = notes;
        return true;
    }

    public bool Skip(string skippedBy)
    {
        if (Status != OnboardingTaskStatusCode.Pending) return false;
        Status = OnboardingTaskStatusCode.Skipped;
        SkippedAt = DateTime.UtcNow;
        SkippedBy = skippedBy;
        return true;
    }

    public bool Reopen(string reopenedBy, string? reason = null)
    {
        if (Status == OnboardingTaskStatusCode.Pending) return false;
        Status = OnboardingTaskStatusCode.Pending;
        ReopenedAt = DateTime.UtcNow;
        ReopenedBy = reopenedBy;
        ReopenedReason = reason;
        return true;
    }

    public bool Reassign(string newUserId, string? newUserDisplayName, string reassignedBy)
    {
        if (Status != OnboardingTaskStatusCode.Pending) return false;
        AssignedUserId = newUserId;
        AssignedUserDisplayNameSnapshot = newUserDisplayName;
        ReassignedAt = DateTime.UtcNow;
        ReassignedBy = reassignedBy;
        return true;
    }

    public bool IsOverdue()
    {
        return Status == OnboardingTaskStatusCode.Pending && DueDate < DateTime.UtcNow;
    }

    public bool IsCompleted() => Status == OnboardingTaskStatusCode.Completed;
    public bool IsSkipped() => Status == OnboardingTaskStatusCode.Skipped;
    public bool IsPending() => Status == OnboardingTaskStatusCode.Pending;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
