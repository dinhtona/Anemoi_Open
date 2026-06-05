namespace Anemoi.Hr.Application.Responses;

public sealed class EmployeePositionHistoryResponse
{
    public string Id { get; set; }
    public string EmployeeId { get; set; }
    public string PositionId { get; set; }
    public string PositionCode { get; set; }
    public string PositionName { get; set; }
    public string OldPositionId { get; set; }
    public string OldPositionCode { get; set; }
    public string OldPositionName { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string ReasonCode { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
