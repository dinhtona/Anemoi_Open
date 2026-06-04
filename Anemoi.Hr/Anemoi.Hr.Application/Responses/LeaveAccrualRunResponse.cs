namespace Anemoi.Hr.Application.Responses;

public sealed class LeaveAccrualRunResponse
{
    public string Id { get; set; }
    public string EmployeeId { get; set; }
    public string LeavePolicyId { get; set; }
    public string YearMonth { get; set; }
    public decimal AccruedDays { get; set; }
    public DateTime CreatedAt { get; set; }
}
