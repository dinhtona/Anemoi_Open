namespace Anemoi.Hr.Application.Responses;

public sealed class EmployeePromotionTimelineResponse
{
    public string Id { get; set; }
    public string EmployeeId { get; set; }
    public string ChangeType { get; set; }
    public string OldValueId { get; set; }
    public string OldValueCode { get; set; }
    public string OldValueName { get; set; }
    public string NewValueId { get; set; }
    public string NewValueCode { get; set; }
    public string NewValueName { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string ReasonCode { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
