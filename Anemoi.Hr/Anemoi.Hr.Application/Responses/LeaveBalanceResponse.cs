namespace Anemoi.Hr.Application.Responses;

public sealed class LeaveBalanceResponse
{
    public string Id { get; set; }
    public string EmployeeId { get; set; }
    public string LeavePolicyId { get; set; }
    public int Year { get; set; }
    public decimal OpeningDays { get; set; }
    public decimal AccruedDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal PendingDays { get; set; }
    public decimal AdjustedDays { get; set; }
    public decimal RemainingDays { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
