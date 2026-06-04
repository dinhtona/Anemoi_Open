using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Leaves;

public sealed class LeaveBalance : ValueObject
{
    public LeaveBalanceId Id { get; set; }
    public EmployeeId EmployeeId { get; set; }
    public LeavePolicyId LeavePolicyId { get; set; }
    public int Year { get; set; }
    public decimal OpeningDays { get; set; }
    public decimal AccruedDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal PendingDays { get; set; }
    public decimal AdjustedDays { get; set; }
    public decimal RemainingDays { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Employee Employee { get; set; }
    public LeavePolicy LeavePolicy { get; set; }
    public List<LeaveTransaction> LeaveTransactions { get; set; } = [];

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
