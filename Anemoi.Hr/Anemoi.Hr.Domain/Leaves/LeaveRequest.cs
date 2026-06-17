using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Leaves;

public sealed class LeaveRequest : ValueObject
{
    public LeaveRequestId Id { get; set; }
    public EmployeeId EmployeeId { get; set; }
    public LeavePolicyId LeavePolicyId { get; set; }
    public EmployeeId ApproverEmployeeId { get; set; }
    public string LeaveTypeCode { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal RequestedDays { get; set; }
    public string StatusCode { get; set; }
    public string Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Employee Employee { get; set; }
    public Employee ApproverEmployee { get; set; }
    public LeavePolicy LeavePolicy { get; set; }
    public List<LeaveTransaction> LeaveTransactions { get; set; } = [];

    public void MarkWorkflowApproved(string approverId)
    {
        StatusCode = LeaveRequestStatusCode.Approved;
        ApproverEmployeeId = new EmployeeId(Guid.Parse(approverId));
    }

    public void MarkWorkflowRejected(string approverId, string? reason)
    {
        StatusCode = LeaveRequestStatusCode.Rejected;
        ApproverEmployeeId = new EmployeeId(Guid.Parse(approverId));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
