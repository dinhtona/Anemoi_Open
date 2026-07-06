using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.MasterData;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Leaves;

public sealed class LeaveAccrualRun : ValueObject
{
    public LeaveAccrualRunId Id { get; set; }
    public EmployeeId EmployeeId { get; set; }
    public LeavePolicyId LeavePolicyId { get; set; }
    public string YearMonth { get; set; }
    public decimal AccruedDays { get; set; }
    public DateTime CreatedAt { get; set; }

    public Employee Employee { get; set; }
    public MasterData.LeavePolicy LeavePolicy { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
