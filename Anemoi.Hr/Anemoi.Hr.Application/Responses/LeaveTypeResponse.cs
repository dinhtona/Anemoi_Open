namespace Anemoi.Hr.Application.Responses;

public sealed class LeaveTypeResponse
{
    public string Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public bool IsPaid { get; set; }
    public bool RequiresApproval { get; set; }
    public decimal AnnualEntitlement { get; set; }
    public bool CarryForwardAllowed { get; set; }
    public decimal MaxCarryForwardDays { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
