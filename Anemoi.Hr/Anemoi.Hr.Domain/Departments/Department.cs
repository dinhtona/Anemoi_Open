using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Departments;

public sealed class Department : ValueObject
{
    public DepartmentId Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string DepartmentTypeCode { get; set; }
    public DepartmentId? ParentDepartmentId { get; set; }
    public EmployeeId? ManagerEmployeeId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Department ParentDepartment { get; set; }
    public List<Department> ChildDepartments { get; set; } = [];
    public Employee ManagerEmployee { get; set; }
    public List<Employee> PrimaryEmployees { get; set; } = [];
    public List<Position> Positions { get; set; } = [];

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
