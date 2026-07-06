namespace Anemoi.Hr.Application.Responses;

public sealed class OnboardingTaskTemplateResponse
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public string? AssigneeRoleCode { get; set; }
    public int OffsetDays { get; set; }
    public int SortOrder { get; set; }
    public bool IsRequired { get; set; }
}
