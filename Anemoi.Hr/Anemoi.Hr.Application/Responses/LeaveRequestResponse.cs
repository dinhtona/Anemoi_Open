namespace Anemoi.Hr.Application.Responses;

public sealed class LeaveRequestResponse
{
    public string Id { get; set; }
    public string EmployeeId { get; set; }
    public string LeavePolicyId { get; set; }
    public string ApproverEmployeeId { get; set; }
    public string ApproverName { get; set; }
    public string LeaveTypeCode { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal RequestedDays { get; set; }
    public string StatusCode { get; set; }
    public string Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
