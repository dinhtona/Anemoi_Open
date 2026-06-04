using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Positions;

public sealed class Position : ValueObject
{
    public PositionId Id { get; set; }
    public DepartmentId DepartmentId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string PositionTypeCode { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Department Department { get; set; }
    public List<Employee> PrimaryEmployees { get; set; } = [];

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
