namespace Anemoi.Hr.Application.Responses;

public sealed class OnboardingPlanTemplateResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public List<OnboardingTaskTemplateResponse> TaskTemplates { get; set; } = [];
}
