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

    public void StartOnboarding(string actor)
    {
        if (EmploymentStatusCode != EmpStatus.PendingOnboarding)
            throw new DomainException($"Cannot start onboarding from status {EmploymentStatusCode}");
        EmploymentStatusCode = EmpStatus.Onboarding;
        UpdatedAt = DateTime.UtcNow;
        AddEvent(new EmployeeStatusChangedDomainEvent(Id, EmpStatus.PendingOnboarding, EmpStatus.Onboarding, actor));
    }

    public void Activate(string actor)
    {
        if (EmploymentStatusCode != EmpStatus.Onboarding && EmploymentStatusCode != EmpStatus.Suspended)
            throw new DomainException($"Cannot activate from status {EmploymentStatusCode}");
        var fromStatus = EmploymentStatusCode;
        EmploymentStatusCode = EmpStatus.Active;
        UpdatedAt = DateTime.UtcNow;
        AddEvent(new EmployeeStatusChangedDomainEvent(Id, fromStatus, EmpStatus.Active, actor));
    }

    public void Suspend(string actor)
    {
        if (EmploymentStatusCode != EmpStatus.Active)
            throw new DomainException($"Cannot suspend from status {EmploymentStatusCode}");
        EmploymentStatusCode = EmpStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
        AddEvent(new EmployeeStatusChangedDomainEvent(Id, EmpStatus.Active, EmpStatus.Suspended, actor));
    }

    public void Resume(string actor)
    {
        if (EmploymentStatusCode != EmpStatus.Suspended)
            throw new DomainException($"Cannot resume from status {EmploymentStatusCode}");
        EmploymentStatusCode = EmpStatus.Active;
        UpdatedAt = DateTime.UtcNow;
        AddEvent(new EmployeeStatusChangedDomainEvent(Id, EmpStatus.Suspended, EmpStatus.Active, actor));
    }

    public void Resign(string actor)
    {
        if (EmploymentStatusCode != EmpStatus.Active)
            throw new DomainException($"Cannot resign from status {EmploymentStatusCode}");
        EmploymentStatusCode = EmpStatus.Resigned;
        UpdatedAt = DateTime.UtcNow;
        AddEvent(new EmployeeStatusChangedDomainEvent(Id, EmpStatus.Active, EmpStatus.Resigned, actor));
    }

    public void Terminate(string actor)
    {
        if (EmploymentStatusCode != EmpStatus.Active)
            throw new DomainException($"Cannot terminate from status {EmploymentStatusCode}");
        EmploymentStatusCode = EmpStatus.Terminated;
        UpdatedAt = DateTime.UtcNow;
        AddEvent(new EmployeeStatusChangedDomainEvent(Id, EmpStatus.Active, EmpStatus.Terminated, actor));
    }

    public void Archive(string actor)
    {
        if (EmploymentStatusCode != EmpStatus.Resigned && EmploymentStatusCode != EmpStatus.Terminated)
            throw new DomainException($"Cannot archive from status {EmploymentStatusCode}");
        var fromStatus = EmploymentStatusCode;
        EmploymentStatusCode = EmpStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
        AddEvent(new EmployeeStatusChangedDomainEvent(Id, fromStatus, EmpStatus.Archived, actor));
    }
}
