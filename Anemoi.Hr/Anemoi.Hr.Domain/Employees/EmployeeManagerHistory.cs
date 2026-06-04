using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Employees;

public sealed class EmployeeManagerHistory : ValueObject
{
    public EmployeeManagerHistoryId Id { get; set; }
    public EmployeeId EmployeeId { get; set; }
    public EmployeeId ManagerEmployeeId { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string ReasonCode { get; set; }
    public DateTime CreatedAt { get; set; }

    public Employee Employee { get; set; }
    public Employee ManagerEmployee { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
