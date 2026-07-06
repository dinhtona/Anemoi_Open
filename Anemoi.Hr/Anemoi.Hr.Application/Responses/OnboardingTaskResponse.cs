namespace Anemoi.Hr.Application.Responses;

public sealed class OnboardingTaskResponse
{
    public string Id { get; set; }
    public string InstanceId { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public string? AssigneeRoleCode { get; set; }
    public string? AssignedUserId { get; set; }
    public string? AssignedUserDisplayName { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime DueDate { get; set; }
    public int SortOrder { get; set; }
    public bool IsRequired { get; set; }
    public string Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }
    public string? CompletedNotes { get; set; }
    public DateTime? SkippedAt { get; set; }
    public string? SkippedBy { get; set; }
    public bool IsOverdue { get; set; }
}
