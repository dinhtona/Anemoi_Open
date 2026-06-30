using System;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Leaves;
using Anemoi.Hr.Domain.MasterData;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.Domain.Workflow;
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
    private static readonly LeaveTypeId AnnualLeaveTypeId =
        new(Guid.Parse("50000000-0000-0000-0000-000000000001"));
    private static readonly WorkflowDefinitionId TransferWorkflowDefId =
        new(Guid.Parse("80000000-0000-0000-0000-000000000001"));
    private static readonly WorkflowDefinitionId SeparationWorkflowDefId =
        new(Guid.Parse("80000000-0000-0000-0000-000000000002"));
    private static readonly WorkflowDefinitionId LeaveWorkflowDefId =
        new(Guid.Parse("80000000-0000-0000-0000-000000000003"));
    private static readonly WorkflowDefinitionId OvertimeWorkflowDefId =
        new(Guid.Parse("80000000-0000-0000-0000-000000000004"));
    private static readonly WorkflowDefinitionId PayrollWorkflowDefId =
        new(Guid.Parse("80000000-0000-0000-0000-000000000005"));
    private static readonly WorkflowDefinitionId RecruitmentWorkflowDefId =
        new(Guid.Parse("80000000-0000-0000-0000-000000000006"));
    private static readonly WorkflowDefinitionId ProbationWorkflowDefId =
        new(Guid.Parse("80000000-0000-0000-0000-000000000007"));

    public static async Task SeedAsync(IServiceScope serviceScope, CancellationToken cancellationToken = default)
    {
        var departmentRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<Department>>();
        var positionRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<Position>>();
        var employeeRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<Employee>>();
        var leavePolicyRepository = serviceScope.ServiceProvider
            .GetRequiredService<ISqlRepository<Anemoi.Hr.Domain.MasterData.LeavePolicy>>();
        var leaveBalanceRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<LeaveBalance>>();
        var unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var now = DateTime.UtcNow;

        var leaveTypeRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<LeaveType>>();
        await SeedLeaveTypesAsync(leaveTypeRepository, now, cancellationToken);

        await SeedDepartmentsAsync(departmentRepository, now, cancellationToken);
        await SeedPositionsAsync(positionRepository, now, cancellationToken);
        await SeedEmployeesAsync(employeeRepository, now, cancellationToken);
        await SeedLeavePolicyAsync(leavePolicyRepository, now, cancellationToken);
        await SeedLeaveBalancesAsync(leaveBalanceRepository, now.Year, now, cancellationToken);
        await SeedWorkflowDefinitionsAsync(serviceScope, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedDepartmentsAsync(
        ISqlRepository<Department> repository,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var departments = new[]
        {
            Department.Create(
                EngineeringDepartmentId, "ENG", "Engineering",
                DepartmentTypeCode.Functional, null, EngineeringManagerEmployeeId),
            Department.Create(
                PeopleDepartmentId, "PEOPLE", "People Operations",
                DepartmentTypeCode.Functional, null, HrManagerEmployeeId)
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
            Position.Create(
                EngineeringManagerPositionId,
                EngineeringDepartmentId,
                "ENG-MGR",
                "Engineering Manager",
                PositionTypeCode.Manager
            ),
            Position.Create(
                SoftwareEngineerPositionId,
                EngineeringDepartmentId,
                "SWE",
                "Software Engineer",
                PositionTypeCode.IndividualContributor
            ),
            Position.Create(
                HrManagerPositionId,
                PeopleDepartmentId,
                "HR-MGR",
                "HR Manager",
                PositionTypeCode.Manager
            ),
            Position.Create(
                HrSpecialistPositionId,
                PeopleDepartmentId,
                "HR-SPEC",
                "HR Specialist",
                PositionTypeCode.IndividualContributor
            ),
            Position.Create(
                SystemAdministratorPositionId,
                PeopleDepartmentId,
                "SYS-ADMIN",
                "System Administrator",
                PositionTypeCode.Administrator
            )
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
                IdentityUserId = Guid.Parse("01000000-0000-0000-74be-08ded0d6c9db"),
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
                IdentityUserId = Guid.Parse("01000000-0000-0000-9384-08ded0d6c9e1"),
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
                IdentityUserId = Guid.Parse("01000000-0000-0000-32da-08ded0d6c9e7"),
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
                IdentityUserId = Guid.Parse("01000000-0000-0000-e9a2-08ded0d6c9ec"),
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
                IdentityUserId = Guid.Parse("01000000-0000-0000-c22b-08ded0d6c9f2"),
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
                IdentityUserId = Guid.Parse("01000000-0000-0000-ddad-08ded0d6c9b8"),
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

    private static async Task SeedLeaveTypesAsync(
        ISqlRepository<LeaveType> repository,
        DateTime now,
        CancellationToken cancellationToken)
    {
        const string annualCode = LeaveTypeCode.Annual;
        if (await repository.ExistByConditionAsync(x => x.Code == annualCode, cancellationToken)) return;

        await repository.CreateOneAsync(LeaveType.Create(
            AnnualLeaveTypeId,
            annualCode,
            "Annual Leave",
            true,
            true,
            15m,
            true,
            5m
        ), cancellationToken);
    }

    private static async Task SeedLeavePolicyAsync(
        ISqlRepository<Anemoi.Hr.Domain.MasterData.LeavePolicy> repository,
        DateTime now,
        CancellationToken cancellationToken)
    {
        const string annualLeaveCode = "DEV-ANNUAL";
        if (await repository.ExistByConditionAsync(x => x.Code == annualLeaveCode, cancellationToken)) return;

        await repository.CreateOneAsync(
            Anemoi.Hr.Domain.MasterData.LeavePolicy.Create(
                AnnualLeavePolicyId,
                annualLeaveCode,
                "Development Annual Leave",
                AnnualLeaveTypeId,
                "",
                15m
            ), cancellationToken);
    }

    private static async Task SeedWorkflowDefinitionsAsync(
        IServiceScope serviceScope,
        CancellationToken cancellationToken)
    {
        var workflowDefinitionRepository = serviceScope.ServiceProvider
            .GetRequiredService<ISqlRepository<WorkflowDefinition>>();

        await SeedDefinitionIfNotExists(workflowDefinitionRepository, cancellationToken,
            "LEAVE-REQUEST", "Leave Request Approval",
            WorkflowConstants.TargetEntityTypes.LeaveRequest,
            LeaveWorkflowDefId, 0x301, 0x302);

        await SeedDefinitionIfNotExists(workflowDefinitionRepository, cancellationToken,
            "OVERTIME-REQUEST", "Overtime Request Approval",
            WorkflowConstants.TargetEntityTypes.OvertimeRequest,
            OvertimeWorkflowDefId, 0x401, 0x402);

        await SeedDefinitionIfNotExists(workflowDefinitionRepository, cancellationToken,
            "PAYROLL-RUN", "Payroll Run Approval",
            WorkflowConstants.TargetEntityTypes.PayrollRun,
            PayrollWorkflowDefId, 0x501, 0x502);

        await SeedDefinitionIfNotExists(workflowDefinitionRepository, cancellationToken,
            "RECRUITMENT-REQUEST", "Recruitment Request Approval",
            WorkflowConstants.TargetEntityTypes.RecruitmentRequest,
            RecruitmentWorkflowDefId, 0x601, 0x602);

        await SeedDefinitionIfNotExists(workflowDefinitionRepository, cancellationToken,
            "EMPLOYEE-TRANSFER", "Employee Transfer Approval",
            WorkflowConstants.TargetEntityTypes.EmployeeTransfer,
            TransferWorkflowDefId, 0x101, 0x102);

        await SeedDefinitionIfNotExists(workflowDefinitionRepository, cancellationToken,
            "EMPLOYEE-SEPARATION", "Employee Separation Approval",
            WorkflowConstants.TargetEntityTypes.EmployeeSeparation,
            SeparationWorkflowDefId, 0x201, 0x202);

        await SeedDefinitionIfNotExists(workflowDefinitionRepository, cancellationToken,
            "PROBATION-RECORD", "Probation Record Approval",
            WorkflowConstants.TargetEntityTypes.ProbationRecord,
            ProbationWorkflowDefId, 0x701, 0x702);
    }

    private static async Task SeedDefinitionIfNotExists(
        ISqlRepository<WorkflowDefinition> repository,
        CancellationToken cancellationToken,
        string code,
        string name,
        string targetEntityType,
        WorkflowDefinitionId defId,
        int step1Suffix,
        int step2Suffix)
    {
        if (await repository.ExistByConditionAsync(
                x => x.Code == code, cancellationToken)) return;

        var steps = new List<WorkflowDefinitionStep>
        {
            WorkflowDefinitionStep.Create(
                new WorkflowDefinitionStepId(Guid.Parse($"80000000-0000-0000-0000-0000{step1Suffix:X8}")),
                defId, 1, ApproverType.DirectManager, null, true),
            WorkflowDefinitionStep.Create(
                new WorkflowDefinitionStepId(Guid.Parse($"80000000-0000-0000-0000-0000{step2Suffix:X8}")),
                defId, 2, ApproverType.HrManager, null, true)
        };

        var def = WorkflowDefinition.Create(
            defId, code, name,
            null, WorkflowTypeCode.Approval, targetEntityType,
            1, steps);
        def.Activate();
        await repository.CreateOneAsync(def, cancellationToken);
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
                Id = new LeaveBalanceId(IdGenerator.NextGuid()),
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
