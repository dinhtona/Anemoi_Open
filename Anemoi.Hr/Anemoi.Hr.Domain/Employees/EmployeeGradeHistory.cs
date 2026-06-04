using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Employees;

public sealed class EmployeeGradeHistory : ValueObject
{
    public EmployeeGradeHistoryId Id { get; set; }
    public EmployeeId EmployeeId { get; set; }
    public string GradeCode { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string ReasonCode { get; set; }
    public DateTime CreatedAt { get; set; }

    public Employee Employee { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
