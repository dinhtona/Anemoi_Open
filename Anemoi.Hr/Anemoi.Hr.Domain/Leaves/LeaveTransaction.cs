using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Leaves;

public sealed class LeaveTransaction : ValueObject
{
    public LeaveTransactionId Id { get; set; }
    public EmployeeId EmployeeId { get; set; }
    public LeavePolicyId LeavePolicyId { get; set; }
    public LeaveBalanceId LeaveBalanceId { get; set; }
    public LeaveRequestId LeaveRequestId { get; set; }
    public string TransactionTypeCode { get; set; }
    public decimal Days { get; set; }
    public decimal BalanceAfterDays { get; set; }
    public string SourceType { get; set; }
    public string SourceId { get; set; }
    public string Reason { get; set; }
    public DateTime CreatedAt { get; set; }

    public Employee Employee { get; set; }
    public LeavePolicy LeavePolicy { get; set; }
    public LeaveBalance LeaveBalance { get; set; }
    public LeaveRequest LeaveRequest { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
