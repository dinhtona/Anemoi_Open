using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Leaves;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Anemoi.Hr.Infrastructure.SeedData;

public static class HrDevSeedData
{
    private static readonly DepartmentId EngineeringDepartmentId =
        new(Guid.Parse("10000000-0000-0000-0000-000000000001"));
    private static readonly DepartmentId PeopleDepartmentId =
        new(Guid.Parse("10000000-0000-0000-0000-000000000002"));
    private static readonly PositionId EngineeringManagerPositionId =
        new(Guid.Parse("20000000-0000-0000-0000-000000000001"));
    private static readonly PositionId SoftwareEngineerPositionId =
        new(Guid.Parse("20000000-0000-0000-0000-000000000002"));
    private static readonly PositionId HrManagerPositionId =
        new(Guid.Parse("20000000-0000-0000-0000-000000000003"));
    private static readonly PositionId HrSpecialistPositionId =
        new(Guid.Parse("20000000-0000-0000-0000-000000000004"));
    private static readonly PositionId SystemAdministratorPositionId =
        new(Guid.Parse("20000000-0000-0000-0000-000000000005"));
    private static readonly EmployeeId EngineeringManagerEmployeeId =
        new(Guid.Parse("30000000-0000-0000-0000-000000000001"));
    private static readonly EmployeeId SoftwareEngineerEmployeeId =
        new(Guid.Parse("30000000-0000-0000-0000-000000000002"));
    private static readonly EmployeeId SeniorEngineerEmployeeId =
        new(Guid.Parse("30000000-0000-0000-0000-000000000003"));
    private static readonly EmployeeId HrManagerEmployeeId =
        new(Guid.Parse("30000000-0000-0000-0000-000000000004"));
    private static readonly EmployeeId HrSpecialistEmployeeId =
        new(Guid.Parse("30000000-0000-0000-0000-000000000005"));
    private static readonly EmployeeId SystemAdministratorEmployeeId =
        new(Guid.Parse("30000000-0000-0000-0000-000000000006"));
    private static readonly LeavePolicyId AnnualLeavePolicyId =
        new(Guid.Parse("40000000-0000-0000-0000-000000000001"));

    public static async Task SeedAsync(IServiceScope serviceScope, CancellationToken cancellationToken = default)
    {
        var departmentRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<Department>>();
        var positionRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<Position>>();
        var employeeRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<Employee>>();
        var leavePolicyRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<LeavePolicy>>();
        var leaveBalanceRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<LeaveBalance>>();
        var unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var now = DateTime.UtcNow;

        await SeedDepartmentsAsync(departmentRepository, now, cancellationToken);
        await SeedPositionsAsync(positionRepository, now, cancellationToken);
        await SeedEmployeesAsync(employeeRepository, now, cancellationToken);
        await SeedLeavePolicyAsync(leavePolicyRepository, now, cancellationToken);
        await SeedLeaveBalancesAsync(leaveBalanceRepository, now.Year, now, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedDepartmentsAsync(
        ISqlRepository<Department> repository,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var departments = new[]
        {
            new Department
            {
                Id = EngineeringDepartmentId,
                Code = "ENG",
                Name = "Engineering",
                DepartmentTypeCode = "functional",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Department
            {
                Id = PeopleDepartmentId,
                Code = "PEOPLE",
                Name = "People Operations",
                DepartmentTypeCode = "functional",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        foreach (var department in departments)
        {
            if (await repository.ExistByConditionAsync(x => x.Code == department.Code, cancellationToken)) continue;
            await repository.CreateOneAsync(department, cancellationToken);
        }
    }

    private static async Task SeedPositionsAsync(
        ISqlRepository<Position> repository,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var positions = new[]
        {
            new Position
            {
                Id = EngineeringManagerPositionId,
                DepartmentId = EngineeringDepartmentId,
                Code = "ENG-MGR",
                Name = "Engineering Manager",
                PositionTypeCode = "manager",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Position
            {
                Id = SoftwareEngineerPositionId,
                DepartmentId = EngineeringDepartmentId,
                Code = "SWE",
                Name = "Software Engineer",
                PositionTypeCode = "individual_contributor",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Position
            {
                Id = HrManagerPositionId,
                DepartmentId = PeopleDepartmentId,
                Code = "HR-MGR",
                Name = "HR Manager",
                PositionTypeCode = "manager",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Position
            {
                Id = HrSpecialistPositionId,
                DepartmentId = PeopleDepartmentId,
                Code = "HR-SPEC",
                Name = "HR Specialist",
                PositionTypeCode = "individual_contributor",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Position
            {
                Id = SystemAdministratorPositionId,
                DepartmentId = PeopleDepartmentId,
                Code = "SYS-ADMIN",
                Name = "System Administrator",
                PositionTypeCode = "administrator",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        foreach (var position in positions)
        {
            if (await repository.ExistByConditionAsync(x => x.Code == position.Code, cancellationToken)) continue;
            await repository.CreateOneAsync(position, cancellationToken);
        }
    }

    private static async Task SeedEmployeesAsync(
        ISqlRepository<Employee> repository,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var employees = new[]
        {
            new Employee
            {
                Id = EngineeringManagerEmployeeId,
                EmployeeCode = "DEV-ENG-001",
                FullName = "Linh Nguyen",
                WorkEmail = "linh.nguyen@anemoi.test",
                PersonalEmail = "linh.nguyen.personal@example.com",
                PhoneNumber = "+84900000001",
                JoinDate = new DateOnly(2024, 1, 8),
                EmploymentStatusCode = EmploymentStatusCode.Active,
                EmploymentTypeCode = "full_time",
                PrimaryDepartmentId = EngineeringDepartmentId,
                PrimaryPositionId = EngineeringManagerPositionId,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Employee
            {
                Id = SoftwareEngineerEmployeeId,
                EmployeeCode = "DEV-ENG-002",
                FullName = "Minh Tran",
                WorkEmail = "minh.tran@anemoi.test",
                PersonalEmail = "minh.tran.personal@example.com",
                PhoneNumber = "+84900000002",
                JoinDate = new DateOnly(2024, 3, 4),
                EmploymentStatusCode = EmploymentStatusCode.Active,
                EmploymentTypeCode = "full_time",
                PrimaryDepartmentId = EngineeringDepartmentId,
                PrimaryPositionId = SoftwareEngineerPositionId,
                DirectManagerEmployeeId = EngineeringManagerEmployeeId,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Employee
            {
                Id = SeniorEngineerEmployeeId,
                EmployeeCode = "DEV-ENG-003",
                FullName = "An Pham",
                WorkEmail = "an.pham@anemoi.test",
                PersonalEmail = "an.pham.personal@example.com",
                PhoneNumber = "+84900000003",
                JoinDate = new DateOnly(2023, 9, 18),
                EmploymentStatusCode = EmploymentStatusCode.Active,
                EmploymentTypeCode = "full_time",
                PrimaryDepartmentId = EngineeringDepartmentId,
                PrimaryPositionId = SoftwareEngineerPositionId,
                DirectManagerEmployeeId = EngineeringManagerEmployeeId,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Employee
            {
                Id = HrManagerEmployeeId,
                EmployeeCode = "DEV-HR-001",
                FullName = "Mai Le",
                WorkEmail = "mai.le@anemoi.test",
                PersonalEmail = "mai.le.personal@example.com",
                PhoneNumber = "+84900000004",
                JoinDate = new DateOnly(2024, 2, 12),
                EmploymentStatusCode = EmploymentStatusCode.Active,
                EmploymentTypeCode = "full_time",
                PrimaryDepartmentId = PeopleDepartmentId,
                PrimaryPositionId = HrManagerPositionId,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Employee
            {
                Id = HrSpecialistEmployeeId,
                EmployeeCode = "DEV-HR-002",
                FullName = "Khoa Do",
                WorkEmail = "khoa.do@anemoi.test",
                PersonalEmail = "khoa.do.personal@example.com",
                PhoneNumber = "+84900000005",
                JoinDate = new DateOnly(2024, 4, 15),
                EmploymentStatusCode = EmploymentStatusCode.Active,
                EmploymentTypeCode = "full_time",
                PrimaryDepartmentId = PeopleDepartmentId,
                PrimaryPositionId = HrSpecialistPositionId,
                DirectManagerEmployeeId = HrManagerEmployeeId,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Employee
            {
                Id = SystemAdministratorEmployeeId,
                EmployeeCode = "DEV-ADMIN-001",
                FullName = "Anemoi Admin",
                WorkEmail = "admin@anemoi.com",
                PersonalEmail = "admin.personal@example.com",
                PhoneNumber = "+84900000006",
                JoinDate = new DateOnly(2024, 1, 1),
                EmploymentStatusCode = EmploymentStatusCode.Active,
                EmploymentTypeCode = "full_time",
                PrimaryDepartmentId = PeopleDepartmentId,
                PrimaryPositionId = SystemAdministratorPositionId,
                DirectManagerEmployeeId = HrManagerEmployeeId,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        foreach (var employee in employees)
        {
            if (await repository.ExistByConditionAsync(x => x.EmployeeCode == employee.EmployeeCode, cancellationToken))
                continue;
            await repository.CreateOneAsync(employee, cancellationToken);
        }
    }

    private static async Task SeedLeavePolicyAsync(
        ISqlRepository<LeavePolicy> repository,
        DateTime now,
        CancellationToken cancellationToken)
    {
        const string annualLeaveCode = "DEV-ANNUAL";
        if (await repository.ExistByConditionAsync(x => x.Code == annualLeaveCode, cancellationToken)) return;

        await repository.CreateOneAsync(new LeavePolicy
        {
            Id = AnnualLeavePolicyId,
            Code = annualLeaveCode,
            Name = "Development Annual Leave",
            LeaveTypeCode = LeaveTypeCode.Annual,
            MonthlyAccrualDays = 1.25m,
            AnnualMaxDays = 15m,
            AllowCarryForward = true,
            MaxCarryForwardDays = 5m,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        }, cancellationToken);
    }

    private static async Task SeedLeaveBalancesAsync(
        ISqlRepository<LeaveBalance> repository,
        int year,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var employeeIds = new[]
        {
            EngineeringManagerEmployeeId,
            SoftwareEngineerEmployeeId,
            SeniorEngineerEmployeeId,
            HrManagerEmployeeId,
            HrSpecialistEmployeeId,
            SystemAdministratorEmployeeId
        };

        foreach (var employeeId in employeeIds)
        {
            var exists = await repository.ExistByConditionAsync(
                x => x.EmployeeId == employeeId && x.LeavePolicyId == AnnualLeavePolicyId && x.Year == year,
                cancellationToken);
            if (exists) continue;

            await repository.CreateOneAsync(new LeaveBalance
            {
                Id = new LeaveBalanceId(Guid.CreateVersion7()),
                EmployeeId = employeeId,
                LeavePolicyId = AnnualLeavePolicyId,
                Year = year,
                OpeningDays = 0m,
                AccruedDays = 15m,
                UsedDays = 0m,
                PendingDays = 0m,
                AdjustedDays = 0m,
                RemainingDays = 15m,
                CreatedAt = now,
                UpdatedAt = now
            }, cancellationToken);
        }
    }
}
