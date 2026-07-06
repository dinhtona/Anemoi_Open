using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Employees;

public sealed class EmployeeOrganizationHistory : Entity<EmployeeOrganizationHistoryId>
{
    public EmployeeId EmployeeId { get; set; }
    public DepartmentId DepartmentId { get; set; }
    public PositionId PositionId { get; set; }
    public EmployeeId? ManagerEmployeeId { get; set; }
    public string GradeCode { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string ChangeReasonCode { get; set; }

    public Employee Employee { get; set; }
    public Department Department { get; set; }
    public Position Position { get; set; }
    public Employee ManagerEmployee { get; set; }
}
