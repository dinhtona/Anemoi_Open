using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees.Events;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.ModelIds.ModelIds;
using EmpStatus = Anemoi.Hr.Domain.Employees.EmploymentStatusCode;

namespace Anemoi.Hr.Domain.Employees;

public sealed class Employee : Entity<EmployeeId>
{
    public Guid? IdentityUserId { get; set; }
    public string EmployeeCode { get; set; }
    public string FullName { get; set; }
    public string WorkEmail { get; set; }
    public string PersonalEmail { get; set; }
    public string PhoneNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public DateOnly JoinDate { get; set; }
    public string EmploymentStatusCode { get; set; }
    public string EmploymentTypeCode { get; set; }
    public string GradeCode { get; set; }
    public DepartmentId PrimaryDepartmentId { get; set; }
    public PositionId PrimaryPositionId { get; set; }
    public EmployeeId? DirectManagerEmployeeId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Department PrimaryDepartment { get; set; }
    public Position PrimaryPosition { get; set; }
    public Employee? DirectManager { get; set; }
    public List<Employee> DirectReports { get; set; } = [];
    public List<EmployeeDepartmentHistory> DepartmentHistories { get; set; } = [];
    public List<EmployeePositionHistory> PositionHistories { get; set; } = [];
    public List<EmployeeGradeHistory> GradeHistories { get; set; } = [];
    public List<EmployeeManagerHistory> ManagerHistories { get; set; } = [];

    private void ChangeStatus(string toStatus, string actor)
    {
        var fromStatus = EmploymentStatusCode;
        if (!EmpStatus.IsValidTransition(fromStatus, toStatus))
            throw new DomainException($"Cannot change status from {fromStatus} to {toStatus}");
        EmploymentStatusCode = toStatus;
        UpdatedAt = DateTime.UtcNow;
        AddEvent(new EmployeeStatusChangedDomainEvent(Id, fromStatus, toStatus, actor));
    }

    public void StartOnboarding(string actor) => ChangeStatus(EmpStatus.Onboarding, actor);

    public void Activate(string actor) => ChangeStatus(EmpStatus.Active, actor);

    public void Suspend(string actor) => ChangeStatus(EmpStatus.Suspended, actor);

    public void Resume(string actor) => ChangeStatus(EmpStatus.Active, actor);

    public void Resign(string actor) => ChangeStatus(EmpStatus.Resigned, actor);

    public void Terminate(string actor) => ChangeStatus(EmpStatus.Terminated, actor);

    public void Archive(string actor) => ChangeStatus(EmpStatus.Archived, actor);
}
