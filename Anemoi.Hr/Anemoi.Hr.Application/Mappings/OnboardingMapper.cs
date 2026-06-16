using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Onboarding;

namespace Anemoi.Hr.Application.Mappings;

public sealed class OnboardingMapper
{
    public OnboardingPlanTemplateResponse ToResponse(OnboardingPlanTemplate template)
    {
        return new OnboardingPlanTemplateResponse
        {
            Id = template.Id.Value.ToString(),
            Name = template.Name,
            Description = template.Description,
            Status = template.Status,
            Version = template.Version,
            CreatedAt = template.CreatedAt,
            CreatedBy = template.CreatedBy,
            UpdatedAt = template.UpdatedAt,
            UpdatedBy = template.UpdatedBy,
            TaskTemplates = template.TaskTemplates.Select(ToResponse).ToList()
        };
    }

    public OnboardingTaskTemplateResponse ToResponse(OnboardingTaskTemplate taskTemplate)
    {
        return new OnboardingTaskTemplateResponse
        {
            Id = taskTemplate.Id.Value.ToString(),
            Title = taskTemplate.Title,
            Description = taskTemplate.Description,
            AssigneeRoleCode = taskTemplate.AssigneeRoleCode,
            OffsetDays = taskTemplate.OffsetDays,
            SortOrder = taskTemplate.SortOrder,
            IsRequired = taskTemplate.IsRequired
        };
    }

    public OnboardingInstanceResponse ToResponse(OnboardingInstance instance)
    {
        var tasks = instance.Tasks
            .OrderBy(t => t.SortOrder)
            .Select(ToResponse)
            .ToList();

        return new OnboardingInstanceResponse
        {
            Id = instance.Id.Value.ToString(),
            EmployeeId = instance.EmployeeId.Value.ToString(),
            TemplateId = instance.TemplateId?.Value.ToString(),
            TemplateName = instance.TemplateName,
            TemplateVersion = instance.TemplateVersion,
            Status = instance.Status,
            StartDate = instance.StartDate,
            CompletedAt = instance.CompletedAt,
            CompletedBy = instance.CompletedBy,
            CancelledAt = instance.CancelledAt,
            CancelledBy = instance.CancelledBy,
            ForceCompleteReason = instance.ForceCompleteReason,
            CreatedAt = instance.CreatedAt,
            CreatedBy = instance.CreatedBy,
            CompletionPercentage = instance.GetCompletionPercentage(),
            TotalTasks = tasks.Count,
            CompletedTasks = tasks.Count(t => t.Status == OnboardingTaskStatusCode.Completed || t.Status == OnboardingTaskStatusCode.Skipped),
            OverdueTasks = tasks.Count(t => t.IsOverdue),
            Tasks = tasks
        };
    }

    public OnboardingTaskResponse ToResponse(OnboardingTask task)
    {
        return new OnboardingTaskResponse
        {
            Id = task.Id.Value.ToString(),
            InstanceId = Guid.Empty.ToString(),
            Title = task.Title,
            Description = task.Description,
            AssigneeRoleCode = task.AssigneeRoleCode,
            AssignedUserId = task.AssignedUserId,
            AssignedUserDisplayName = task.AssignedUserDisplayNameSnapshot,
            AssignedAt = task.AssignedAt,
            DueDate = task.DueDate,
            SortOrder = task.SortOrder,
            IsRequired = task.IsRequired,
            Status = task.Status,
            CompletedAt = task.CompletedAt,
            CompletedBy = task.CompletedBy,
            CompletedNotes = task.CompletedNotes,
            SkippedAt = task.SkippedAt,
            SkippedBy = task.SkippedBy,
            IsOverdue = task.IsOverdue()
        };
    }

    public OnboardingDashboardResponse ToDashboardResponse(
        int totalActive,
        int totalOverdue,
        int totalPending,
        int totalUpcoming,
        int completedThisMonth,
        double avgCompletionRate)
    {
        return new OnboardingDashboardResponse
        {
            TotalActiveOnboardings = totalActive,
            TotalOverdueTasks = totalOverdue,
            TotalPendingTasks = totalPending,
            TotalUpcomingTasks = totalUpcoming,
            CompletedThisMonth = completedThisMonth,
            AverageCompletionRate = avgCompletionRate
        };
    }
}
