using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Departments;

public sealed class Department : Entity<DepartmentId>
{
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string DepartmentTypeCode { get; private set; }
    public DepartmentId? ParentDepartmentId { get; private set; }
    public EmployeeId? ManagerEmployeeId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Department ParentDepartment { get; private set; }
    public List<Department> ChildDepartments { get; private set; } = [];
    public Employee ManagerEmployee { get; private set; }
    public List<Employee> PrimaryEmployees { get; private set; } = [];
    public List<Position> Positions { get; private set; } = [];

    private Department() { }

    public static Department Create(
        DepartmentId id,
        string code,
        string name,
        string departmentTypeCode,
        DepartmentId? parentDepartmentId,
        EmployeeId? managerEmployeeId)
    {
        return new Department
        {
            Id = id,
            Code = code,
            Name = name,
            DepartmentTypeCode = departmentTypeCode,
            ParentDepartmentId = parentDepartmentId,
            ManagerEmployeeId = managerEmployeeId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateInfo(
        string code,
        string name,
        string departmentTypeCode,
        DepartmentId? parentDepartmentId,
        EmployeeId? managerEmployeeId)
    {
        Code = code;
        Name = name;
        DepartmentTypeCode = departmentTypeCode;
        ParentDepartmentId = parentDepartmentId;
        ManagerEmployeeId = managerEmployeeId;
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
