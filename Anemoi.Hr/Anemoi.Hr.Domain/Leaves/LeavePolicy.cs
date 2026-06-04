using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Leaves;

public sealed class LeavePolicy : ValueObject
{
    public LeavePolicyId Id { get; set; }
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

    public List<LeaveBalance> LeaveBalances { get; set; } = [];
    public List<LeaveRequest> LeaveRequests { get; set; } = [];
    public List<LeaveTransaction> LeaveTransactions { get; set; } = [];
    public List<LeaveAccrualRun> LeaveAccrualRuns { get; set; } = [];

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
