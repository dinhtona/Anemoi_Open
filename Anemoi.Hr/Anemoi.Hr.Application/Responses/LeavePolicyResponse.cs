namespace Anemoi.Hr.Application.Responses;

public sealed class LeavePolicyResponse
{
    public string Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string LeaveTypeCode { get; set; }
    public decimal MonthlyAccrualDays { get; set; }
    public decimal AnnualMaxDays { get; set; }
    public bool AllowCarryForward { get; set; }
    public decimal MaxCarryForwardDays { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
