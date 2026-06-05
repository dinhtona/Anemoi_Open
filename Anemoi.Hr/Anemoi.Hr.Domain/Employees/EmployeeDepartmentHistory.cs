using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Employees;

public sealed class EmployeeDepartmentHistory : ValueObject
{
    public EmployeeDepartmentHistoryId Id { get; set; }
    public EmployeeId EmployeeId { get; set; }
    public DepartmentId DepartmentId { get; set; }
    public DepartmentId OldDepartmentId { get; set; }
    public bool IsPrimary { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string ReasonCode { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public Employee Employee { get; set; }
    public Department Department { get; set; }
    public Department OldDepartment { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
