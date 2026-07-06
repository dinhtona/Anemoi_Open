namespace Anemoi.Hr.Application.Responses;

public sealed class OnboardingInstanceResponse
{
    public string Id { get; set; }
    public string EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeCode { get; set; }
    public string? TemplateId { get; set; }
    public string TemplateName { get; set; }
    public int? TemplateVersion { get; set; }
    public string Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancelledBy { get; set; }
    public string? ForceCompleteReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public double CompletionPercentage { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public List<OnboardingTaskResponse> Tasks { get; set; } = [];
}
