using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Positions;

public sealed class Position : Entity<PositionId>
{
    public DepartmentId DepartmentId { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string PositionTypeCode { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Department Department { get; private set; }
    public List<Employee> PrimaryEmployees { get; private set; } = [];

    private Position() { }

    public static Position Create(PositionId id, DepartmentId departmentId, string code, string name,
        string positionTypeCode)
    {
        return new Position
        {
            Id = id,
            DepartmentId = departmentId,
            Code = code,
            Name = name,
            PositionTypeCode = positionTypeCode,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateInfo(string code, string name, string positionTypeCode, DepartmentId departmentId)
    {
        Code = code;
        Name = name;
        PositionTypeCode = positionTypeCode;
        DepartmentId = departmentId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
