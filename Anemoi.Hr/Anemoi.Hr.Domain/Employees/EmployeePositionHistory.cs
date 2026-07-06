using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Employees;

public sealed class EmployeePositionHistory : ValueObject
{
    public EmployeePositionHistoryId Id { get; set; }
    public EmployeeId EmployeeId { get; set; }
    public PositionId PositionId { get; set; }
    public PositionId OldPositionId { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string ReasonCode { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public Employee Employee { get; set; }
    public Position Position { get; set; }
    public Position OldPosition { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
