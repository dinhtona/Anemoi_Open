using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Onboarding;

public sealed class OnboardingInstance : ValueObject
{
    private readonly List<OnboardingTask> _tasks = [];

    public OnboardingInstanceId Id { get; private set; }
    public EmployeeId EmployeeId { get; private set; }
    public OnboardingPlanTemplateId? TemplateId { get; private set; }
    public string TemplateName { get; private set; }
    public int? TemplateVersion { get; private set; }
    public string Status { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? CompletedBy { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancelledBy { get; private set; }
    public DateTime? ForceCompletedAt { get; private set; }
    public string? ForceCompletedBy { get; private set; }
    public string? ForceCompleteReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public string UpdatedBy { get; private set; }
    public IReadOnlyList<OnboardingTask> Tasks => _tasks.AsReadOnly();

    private OnboardingInstance() { }

    private OnboardingInstance(
        OnboardingInstanceId id,
        EmployeeId employeeId,
        OnboardingPlanTemplateId? templateId,
        string templateName,
        int? templateVersion,
        DateTime startDate,
        string createdBy)
    {
        Id = id;
        EmployeeId = employeeId;
        TemplateId = templateId;
        TemplateName = templateName;
        TemplateVersion = templateVersion;
        Status = OnboardingInstanceStatusCode.InProgress;
        StartDate = startDate;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        UpdatedAt = CreatedAt;
        UpdatedBy = createdBy;
    }

    public static OnboardingInstance Create(
        OnboardingInstanceId id,
        EmployeeId employeeId,
        OnboardingPlanTemplateId? templateId,
        string templateName,
        int? templateVersion,
        DateTime startDate,
        string createdBy)
    {
        return new OnboardingInstance(
            id, employeeId, templateId, templateName, templateVersion,
            startDate, createdBy);
    }

    public void AddTask(OnboardingTask task)
    {
        _tasks.Add(task);
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CompleteTask(OnboardingTaskId taskId, string completedBy, string? notes = null)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null || !task.Complete(completedBy, notes)) return false;

        CheckAutoCompletion();
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = completedBy;
        return true;
    }

    public bool SkipTask(OnboardingTaskId taskId, string skippedBy)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null || !task.Skip(skippedBy)) return false;

        CheckAutoCompletion();
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = skippedBy;
        return true;
    }

    public bool ReopenTask(OnboardingTaskId taskId, string reopenedBy, string? reason = null)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null || !task.Reopen(reopenedBy, reason)) return false;

        if (Status == OnboardingInstanceStatusCode.Completed)
        {
            Status = OnboardingInstanceStatusCode.InProgress;
            CompletedAt = null;
            CompletedBy = null;
        }

        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = reopenedBy;
        return true;
    }

    public bool ReassignTask(OnboardingTaskId taskId, string newUserId, string? newUserDisplayName, string reassignedBy)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null || !task.Reassign(newUserId, newUserDisplayName, reassignedBy)) return false;

        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = reassignedBy;
        return true;
    }

    public bool Cancel(string cancelledBy)
    {
        if (Status != OnboardingInstanceStatusCode.InProgress) return false;
        Status = OnboardingInstanceStatusCode.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancelledBy = cancelledBy;
        UpdatedAt = CancelledAt.Value;
        UpdatedBy = cancelledBy;
        return true;
    }

    public bool Reopen(string reopenedBy)
    {
        if (Status != OnboardingInstanceStatusCode.Completed &&
            Status != OnboardingInstanceStatusCode.Cancelled) return false;
        Status = OnboardingInstanceStatusCode.InProgress;
        CompletedAt = null;
        CompletedBy = null;
        CancelledAt = null;
        CancelledBy = null;
        ForceCompletedAt = null;
        ForceCompletedBy = null;
        ForceCompleteReason = null;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = reopenedBy;
        return true;
    }

    public bool ForceComplete(string completedBy, string reason)
    {
        if (Status != OnboardingInstanceStatusCode.InProgress) return false;
        Status = OnboardingInstanceStatusCode.Completed;
        ForceCompletedAt = DateTime.UtcNow;
        ForceCompletedBy = completedBy;
        ForceCompleteReason = reason;
        CompletedAt = ForceCompletedAt;
        CompletedBy = completedBy;
        UpdatedAt = ForceCompletedAt.Value;
        UpdatedBy = completedBy;
        return true;
    }

    public double GetCompletionPercentage()
    {
        if (_tasks.Count == 0) return 0;
        return (double)_tasks.Count(t => t.IsCompleted() || t.IsSkipped()) / _tasks.Count * 100;
    }

    private void CheckAutoCompletion()
    {
        if (_tasks.Count > 0 && _tasks.All(t => t.IsCompleted() || t.IsSkipped()))
        {
            Status = OnboardingInstanceStatusCode.Completed;
            CompletedAt = DateTime.UtcNow;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
