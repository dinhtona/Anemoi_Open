using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Domain.Models;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.AttendanceCommands.LockAttendancePeriod;
using Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.CalculatePayrollRun;
using Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.LockPayrollPeriod;
using Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.RecalculatePayrollRun;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Attendance;
using Anemoi.Hr.Domain.Compensation;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Insurance;
using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.Domain.Taxation;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging.Abstractions;
using OneOf;
using Xunit;

namespace Anemoi.BuildingBlock.Test;

public sealed class HrAttendancePayrollIntegrationTests
{
    private static Employee CreateEmployee(EmployeeId id)
    {
        return new Employee
        {
            Id = id,
            EmployeeCode = "EMP-" + id.Value.ToString("N")[..6].ToUpper(),
            FullName = "John Doe",
            JoinDate = new DateOnly(2025, 1, 1),
            EmploymentStatusCode = "active",
            EmploymentTypeCode = "full_time",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static EmployeeSalary CreateSalary(EmployeeId employeeId, decimal baseSalary, SalaryType type)
    {
        return new EmployeeSalary
        {
            Id = new EmployeeSalaryId(Guid.NewGuid()),
            EmployeeId = employeeId,
            BaseSalary = baseSalary,
            SalaryType = type,
            Currency = "USD",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            Reason = CompensationChangeReason.Hired,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test"
        };
    }

    private static AttendancePeriod CreateAttendancePeriod(AttendancePeriodId id)
    {
        return new AttendancePeriod
        {
            Id = id,
            PeriodCode = "202605",
            StatusCode = "Draft",
            StartDate = new DateOnly(2026, 5, 1),
            EndDate = new DateOnly(2026, 5, 31),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test",
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "test"
        };
    }

    private static AttendanceRecord CreateAttendanceRecord(
        AttendancePeriodId attendancePeriodId,
        EmployeeId employeeId,
        DateOnly workDate,
        decimal workedDays,
        decimal workedHours,
        string status)
    {
        return new AttendanceRecord
        {
            Id = new AttendanceRecordId(Guid.NewGuid()),
            AttendancePeriodId = attendancePeriodId,
            EmployeeId = employeeId,
            WorkDate = workDate,
            WorkedDays = workedDays,
            WorkedHours = workedHours,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test",
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "test"
        };
    }

    private static LockAttendancePeriodHandler CreateLockAttendancePeriodHandler(
        AttendancePeriod period,
        List<AttendanceRecord> records,
        List<AttendanceSummary> summaries,
        IUnitOfWork unitOfWork = null)
    {
        return new LockAttendancePeriodHandler(
            new FakeRepository<AttendancePeriod>([period]),
            new FakeRepository<AttendanceRecord>(records),
            new FakeRepository<AttendanceSummary>(summaries),
            unitOfWork ?? new FakeUnitOfWork(),
            new AttendanceMapper(),
            NullLogger<LockAttendancePeriodHandler>.Instance);
    }

    [Fact]
    public async Task LockAttendancePeriod_CreatesAttendanceSummaries()
    {
        var attendancePeriodId = new AttendancePeriodId(Guid.NewGuid());
        var employeeId = new EmployeeId(Guid.NewGuid());
        var period = CreateAttendancePeriod(attendancePeriodId);
        var summaries = new List<AttendanceSummary>();
        var records = new List<AttendanceRecord>
        {
            CreateAttendanceRecord(attendancePeriodId, employeeId, new DateOnly(2026, 5, 5), 1, 8, AttendanceStatusCodes.Present),
            CreateAttendanceRecord(attendancePeriodId, employeeId, new DateOnly(2026, 5, 6), 0.5m, 4, AttendanceStatusCodes.Leave)
        };

        var handler = CreateLockAttendancePeriodHandler(period, records, summaries);

        var result = await handler.Handle(
            new LockAttendancePeriodCommand(attendancePeriodId, true, "test"),
            CancellationToken.None);

        Assert.True(result.IsT0);
        Assert.Equal("Locked", period.StatusCode);
        var summary = Assert.Single(summaries);
        Assert.Equal(attendancePeriodId, summary.AttendancePeriodId);
        Assert.Equal(employeeId, summary.EmployeeId);
        Assert.Equal(1.5m, summary.WorkedDays);
        Assert.Equal(12, summary.WorkedHours);
        Assert.Equal(0.5m, summary.LeaveDays);
        Assert.Equal(1m, summary.PaidWorkingDays);
        Assert.Equal(0.5m, summary.PaidLeaveDays);
        Assert.Equal(0, summary.UnpaidLeaveDays);
    }

    [Fact]
    public async Task LockAttendancePeriod_MultipleEmployeesCreateSeparateSummaries()
    {
        var attendancePeriodId = new AttendancePeriodId(Guid.NewGuid());
        var firstEmployeeId = new EmployeeId(Guid.NewGuid());
        var secondEmployeeId = new EmployeeId(Guid.NewGuid());
        var period = CreateAttendancePeriod(attendancePeriodId);
        var summaries = new List<AttendanceSummary>();
        var records = new List<AttendanceRecord>
        {
            CreateAttendanceRecord(attendancePeriodId, firstEmployeeId, new DateOnly(2026, 5, 5), 1, 8, AttendanceStatusCodes.Present),
            CreateAttendanceRecord(attendancePeriodId, secondEmployeeId, new DateOnly(2026, 5, 5), 0, 0, AttendanceStatusCodes.Absent)
        };

        var handler = CreateLockAttendancePeriodHandler(period, records, summaries);

        var result = await handler.Handle(
            new LockAttendancePeriodCommand(attendancePeriodId, true, "test"),
            CancellationToken.None);

        Assert.True(result.IsT0);
        Assert.Equal(2, summaries.Count);
        Assert.Contains(summaries, x => x.EmployeeId == firstEmployeeId);
        Assert.Contains(summaries, x => x.EmployeeId == secondEmployeeId);
    }

    [Fact]
    public async Task LockAttendancePeriod_EmptyPeriodReturnsRecordsNotFound()
    {
        var attendancePeriodId = new AttendancePeriodId(Guid.NewGuid());
        var period = CreateAttendancePeriod(attendancePeriodId);

        var handler = CreateLockAttendancePeriodHandler(period, [], []);

        var result = await handler.Handle(
            new LockAttendancePeriodCommand(attendancePeriodId, true, "test"),
            CancellationToken.None);

        Assert.True(result.IsT1);
        Assert.Equal(HrBusinessErrorCodes.AttendanceRecordsNotFoundForPeriod, result.AsT1.Code);
        Assert.Equal("Draft", period.StatusCode);
    }

    [Fact]
    public async Task LockAttendancePeriod_ExistingSummaryReturnsAlreadyExistsWithoutMutation()
    {
        var attendancePeriodId = new AttendancePeriodId(Guid.NewGuid());
        var employeeId = new EmployeeId(Guid.NewGuid());
        var period = CreateAttendancePeriod(attendancePeriodId);
        var existingSummary = new AttendanceSummary
        {
            Id = new AttendanceSummaryId(Guid.NewGuid()),
            AttendancePeriodId = attendancePeriodId,
            EmployeeId = employeeId,
            WorkedDays = 20,
            WorkedHours = 160,
            PaidWorkingDays = 20,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            CreatedBy = "original",
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedBy = "original"
        };
        var records = new List<AttendanceRecord>
        {
            CreateAttendanceRecord(attendancePeriodId, employeeId, new DateOnly(2026, 5, 5), 1, 8, AttendanceStatusCodes.Present)
        };

        var handler = CreateLockAttendancePeriodHandler(period, records, [existingSummary]);

        var result = await handler.Handle(
            new LockAttendancePeriodCommand(attendancePeriodId, true, "test"),
            CancellationToken.None);

        Assert.True(result.IsT1);
        Assert.Equal(HrBusinessErrorCodes.AttendanceSummaryAlreadyExists, result.AsT1.Code);
        Assert.Equal("Draft", period.StatusCode);
        Assert.Equal(20, existingSummary.WorkedDays);
        Assert.Equal(160, existingSummary.WorkedHours);
        Assert.Equal("original", existingSummary.UpdatedBy);
    }

    [Fact]
    public async Task LockAttendancePeriod_SaveFailureLeavesPeriodUnlocked()
    {
        var attendancePeriodId = new AttendancePeriodId(Guid.NewGuid());
        var employeeId = new EmployeeId(Guid.NewGuid());
        var period = CreateAttendancePeriod(attendancePeriodId);
        var summaries = new List<AttendanceSummary>();
        var records = new List<AttendanceRecord>
        {
            CreateAttendanceRecord(attendancePeriodId, employeeId, new DateOnly(2026, 5, 5), 1, 8, AttendanceStatusCodes.Present)
        };
        var handler = CreateLockAttendancePeriodHandler(
            period,
            records,
            summaries,
            new FailingUnitOfWork());

        var result = await handler.Handle(
            new LockAttendancePeriodCommand(attendancePeriodId, true, "test"),
            CancellationToken.None);

        Assert.True(result.IsT1);
        Assert.Equal(HrBusinessErrorCodes.AttendanceSummaryGenerationFailed, result.AsT1.Code);
        Assert.Equal("Draft", period.StatusCode);
    }

    // ===== LockPayrollPeriod Tests =====

    [Fact]
    public async Task LockPayrollPeriod_NoAttendancePeriodId_Returns_AttendancePeriodRequired()
    {
        var payrollPeriodId = new PayrollPeriodId(Guid.NewGuid());

        var period = new PayrollPeriod
        {
            Id = payrollPeriodId,
            PeriodCode = "202605",
            StatusCode = "Draft",
            StartDate = new DateOnly(2026, 5, 1),
            EndDate = new DateOnly(2026, 5, 31),
            StandardWorkingDays = 22,
            CreatedBy = "test",
            UpdatedBy = "test"
        };
        typeof(PayrollPeriod).GetProperty(nameof(PayrollPeriod.AttendancePeriodId))
            ?.SetValue(period, null);

        var handler = new LockPayrollPeriodHandler(
            new FakeRepository<PayrollPeriod>([period]),
            new FakeRepository<AttendancePeriod>([]),
            new FakeUnitOfWork(),
            new PayrollMapper()
        );

        var result = await handler.Handle(
            new LockPayrollPeriodCommand(payrollPeriodId, true, "test"),
            CancellationToken.None
        );

        Assert.True(result.IsT1);
        Assert.Equal(HrBusinessErrorCodes.PayrollPeriodAttendancePeriodRequired, result.AsT1.Code);
    }

    [Fact]
    public async Task LockPayrollPeriod_AttendancePeriodNotFound_Returns_AttendancePeriodNotFound()
    {
        var payrollPeriodId = new PayrollPeriodId(Guid.NewGuid());
        var attendancePeriodId = new AttendancePeriodId(Guid.NewGuid());

        var period = new PayrollPeriod
        {
            Id = payrollPeriodId,
            PeriodCode = "202605",
            StatusCode = "Draft",
            StartDate = new DateOnly(2026, 5, 1),
            EndDate = new DateOnly(2026, 5, 31),
            StandardWorkingDays = 22,
            CreatedBy = "test",
            UpdatedBy = "test"
        };
        typeof(PayrollPeriod).GetProperty(nameof(PayrollPeriod.AttendancePeriodId))
            ?.SetValue(period, attendancePeriodId);

        var handler = new LockPayrollPeriodHandler(
            new FakeRepository<PayrollPeriod>([period]),
            new FakeRepository<AttendancePeriod>([]), // No attendance period
            new FakeUnitOfWork(),
            new PayrollMapper()
        );

        var result = await handler.Handle(
            new LockPayrollPeriodCommand(payrollPeriodId, true, "test"),
            CancellationToken.None
        );

        Assert.True(result.IsT1);
        Assert.Equal(HrBusinessErrorCodes.AttendancePeriodNotFound, result.AsT1.Code);
    }

    [Fact]
    public async Task LockPayrollPeriod_DoesNotCreateOrUpdateAttendanceSummaries()
    {
        var payrollPeriodId = new PayrollPeriodId(Guid.NewGuid());
        var attendancePeriodId = new AttendancePeriodId(Guid.NewGuid());

        var period = new PayrollPeriod
        {
            Id = payrollPeriodId,
            PeriodCode = "202605",
            StatusCode = "Draft",
            StartDate = new DateOnly(2026, 5, 1),
            EndDate = new DateOnly(2026, 5, 31),
            StandardWorkingDays = 22,
            CreatedBy = "test",
            UpdatedBy = "test"
        };
        typeof(PayrollPeriod).GetProperty(nameof(PayrollPeriod.AttendancePeriodId))
            ?.SetValue(period, attendancePeriodId);

        var attPeriod = new AttendancePeriod
        {
            Id = attendancePeriodId,
            PeriodCode = "202605",
            StatusCode = "Locked",
            StartDate = new DateOnly(2026, 5, 1),
            EndDate = new DateOnly(2026, 5, 31),
            CreatedBy = "test",
            UpdatedBy = "test"
        };

        var existingSummary = new AttendanceSummary
        {
            Id = new AttendanceSummaryId(Guid.NewGuid()),
            AttendancePeriodId = attendancePeriodId,
            EmployeeId = new EmployeeId(Guid.NewGuid()),
            WorkedDays = 20,
            WorkedHours = 160,
            UpdatedBy = "original"
        };

        var handler = new LockPayrollPeriodHandler(
            new FakeRepository<PayrollPeriod>([period]),
            new FakeRepository<AttendancePeriod>([attPeriod]),
            new FakeUnitOfWork(),
            new PayrollMapper());

        var result = await handler.Handle(
            new LockPayrollPeriodCommand(payrollPeriodId, true, "test"),
            CancellationToken.None
        );

        Assert.True(result.IsT0);
        Assert.Equal("Locked", period.StatusCode);
        Assert.Equal(20, existingSummary.WorkedDays);
        Assert.Equal(160, existingSummary.WorkedHours);
        Assert.Equal("original", existingSummary.UpdatedBy);
    }

    // ===== CalculatePayrollRun Tests =====

    [Fact]
    public async Task CalculatePayrollRun_MissingAttendancePeriodId_Returns_NotLinked()
    {
        var employeeId = new EmployeeId(Guid.NewGuid());
        var payrollPeriodId = new PayrollPeriodId(Guid.NewGuid());

        var period = new PayrollPeriod
        {
            Id = payrollPeriodId,
            PeriodCode = "202605",
            StatusCode = "Draft",
            StartDate = new DateOnly(2026, 5, 1),
            EndDate = new DateOnly(2026, 5, 31),
            StandardWorkingDays = 22,
            CreatedBy = "test",
            UpdatedBy = "test"
        };
        // Explicitly set AttendancePeriodId to null to simulate no linkage
        typeof(PayrollPeriod).GetProperty(nameof(PayrollPeriod.AttendancePeriodId))
            ?.SetValue(period, null);

        var handler = new CalculatePayrollRunHandler(
            new FakeRepository<PayrollPeriod>([period]),
            new FakeRepository<PayrollRun>([]),
            new FakeRepository<Employee>([CreateEmployee(employeeId)]),
            new FakeRepository<EmployeeSalary>([CreateSalary(employeeId, 3000, SalaryType.Monthly)]),
            new FakeRepository<EmployeeAllowance>([]),
            new FakeRepository<AttendancePeriod>([]),
            new FakeRepository<AttendanceSummary>([]),
            new FakeRepository<TaxCalculationSnapshot>([]),
            new FakeRepository<InsuranceCalculationSnapshot>([]),
            new FakeOvertimeSnapshotProvider(),
            new FakeUnitOfWork(),
            new PayrollMapper()
        );

        var result = await handler.Handle(
            new CalculatePayrollRunCommand(payrollPeriodId, employeeId, true, "system"),
            CancellationToken.None
        );

        Assert.True(result.IsT1);
        Assert.Equal(HrBusinessErrorCodes.PayrollAttendancePeriodNotLinked, result.AsT1.Code);
    }

    [Fact]
    public async Task CalculatePayrollRun_UnlockedAttendancePeriod_Returns_NotLocked()
    {
        var employeeId = new EmployeeId(Guid.NewGuid());
        var payrollPeriodId = new PayrollPeriodId(Guid.NewGuid());
        var attendancePeriodId = new AttendancePeriodId(Guid.NewGuid());

        var period = new PayrollPeriod
        {
            Id = payrollPeriodId,
            PeriodCode = "202605",
            StatusCode = "Draft",
            StartDate = new DateOnly(2026, 5, 1),
            EndDate = new DateOnly(2026, 5, 31),
            StandardWorkingDays = 22,
            CreatedBy = "test",
            UpdatedBy = "test"
        };
        typeof(PayrollPeriod).GetProperty(nameof(PayrollPeriod.AttendancePeriodId))
            ?.SetValue(period, attendancePeriodId);

        var attPeriod = new AttendancePeriod
        {
            Id = attendancePeriodId,
            PeriodCode = "202605",
            StatusCode = "Draft", // Unlocked
            StartDate = new DateOnly(2026, 5, 1),
            EndDate = new DateOnly(2026, 5, 31),
            CreatedBy = "test",
            UpdatedBy = "test"
        };

        var handler = new CalculatePayrollRunHandler(
            new FakeRepository<PayrollPeriod>([period]),
            new FakeRepository<PayrollRun>([]),
            new FakeRepository<Employee>([CreateEmployee(employeeId)]),
            new FakeRepository<EmployeeSalary>([CreateSalary(employeeId, 3000, SalaryType.Monthly)]),
            new FakeRepository<EmployeeAllowance>([]),
            new FakeRepository<AttendancePeriod>([attPeriod]),
            new FakeRepository<AttendanceSummary>([]),
            new FakeRepository<TaxCalculationSnapshot>([]),
            new FakeRepository<InsuranceCalculationSnapshot>([]),
            new FakeOvertimeSnapshotProvider(),
            new FakeUnitOfWork(),
            new PayrollMapper()
        );

        var result = await handler.Handle(
            new CalculatePayrollRunCommand(payrollPeriodId, employeeId, true, "system"),
            CancellationToken.None
        );

        Assert.True(result.IsT1);
        Assert.Equal(HrBusinessErrorCodes.AttendancePeriodNotLocked, result.AsT1.Code);
    }

    [Fact]
    public async Task CalculatePayrollRun_MissingAttendanceSummary_Returns_SummaryNotFound()
    {
        var employeeId = new EmployeeId(Guid.NewGuid());
        var payrollPeriodId = new PayrollPeriodId(Guid.NewGuid());
        var attendancePeriodId = new AttendancePeriodId(Guid.NewGuid());

        var period = new PayrollPeriod
        {
            Id = payrollPeriodId,
            PeriodCode = "202605",
            StatusCode = "Draft",
            StartDate = new DateOnly(2026, 5, 1),
            EndDate = new DateOnly(2026, 5, 31),
            StandardWorkingDays = 22,
            CreatedBy = "test",
            UpdatedBy = "test"
        };
        typeof(PayrollPeriod).GetProperty(nameof(PayrollPeriod.AttendancePeriodId))
            ?.SetValue(period, attendancePeriodId);

        var attPeriod = new AttendancePeriod
        {
            Id = attendancePeriodId,
            PeriodCode = "202605",
            StatusCode = "Locked",
            StartDate = new DateOnly(2026, 5, 1),
            EndDate = new DateOnly(2026, 5, 31),
            CreatedBy = "test",
            UpdatedBy = "test"
        };

        var handler = new CalculatePayrollRunHandler(
            new FakeRepository<PayrollPeriod>([period]),
            new FakeRepository<PayrollRun>([]),
            new FakeRepository<Employee>([CreateEmployee(employeeId)]),
            new FakeRepository<EmployeeSalary>([CreateSalary(employeeId, 3000, SalaryType.Monthly)]),
            new FakeRepository<EmployeeAllowance>([]),
            new FakeRepository<AttendancePeriod>([attPeriod]),
            new FakeRepository<AttendanceSummary>([]), // No summary
            new FakeRepository<TaxCalculationSnapshot>([]),
            new FakeRepository<InsuranceCalculationSnapshot>([]),
            new FakeOvertimeSnapshotProvider(),
            new FakeUnitOfWork(),
            new PayrollMapper()
        );

        var result = await handler.Handle(
            new CalculatePayrollRunCommand(payrollPeriodId, employeeId, true, "system"),
            CancellationToken.None
        );

        Assert.True(result.IsT1);
        Assert.Equal(HrBusinessErrorCodes.AttendanceSummaryNotFound, result.AsT1.Code);
    }

    [Fact]
    public async Task CalculatePayrollRun_SucceedsAfterAttendanceLockGeneratesSummary()
    {
        var employeeId = new EmployeeId(Guid.NewGuid());
        var payrollPeriodId = new PayrollPeriodId(Guid.NewGuid());
        var attendancePeriodId = new AttendancePeriodId(Guid.NewGuid());
        var attendancePeriod = CreateAttendancePeriod(attendancePeriodId);
        var summaries = new List<AttendanceSummary>();
        var records = new List<AttendanceRecord>
        {
            CreateAttendanceRecord(attendancePeriodId, employeeId, new DateOnly(2026, 5, 5), 1, 8, AttendanceStatusCodes.Present),
            CreateAttendanceRecord(attendancePeriodId, employeeId, new DateOnly(2026, 5, 6), 1, 8, AttendanceStatusCodes.Present)
        };

        var lockHandler = CreateLockAttendancePeriodHandler(attendancePeriod, records, summaries);
        var lockResult = await lockHandler.Handle(
            new LockAttendancePeriodCommand(attendancePeriodId, true, "test"),
            CancellationToken.None);

        Assert.True(lockResult.IsT0);

        var payrollPeriod = new PayrollPeriod
        {
            Id = payrollPeriodId,
            PeriodCode = "202605",
            StatusCode = "Draft",
            StartDate = new DateOnly(2026, 5, 1),
            EndDate = new DateOnly(2026, 5, 31),
            StandardWorkingDays = 22,
            AttendancePeriodId = attendancePeriodId,
            CreatedBy = "test",
            UpdatedBy = "test"
        };
        var payrollRuns = new List<PayrollRun>();
        var calculateHandler = new CalculatePayrollRunHandler(
            new FakeRepository<PayrollPeriod>([payrollPeriod]),
            new FakeRepository<PayrollRun>(payrollRuns),
            new FakeRepository<Employee>([CreateEmployee(employeeId)]),
            new FakeRepository<EmployeeSalary>([CreateSalary(employeeId, 2200, SalaryType.Monthly)]),
            new FakeRepository<EmployeeAllowance>([]),
            new FakeRepository<AttendancePeriod>([attendancePeriod]),
            new FakeRepository<AttendanceSummary>(summaries),
            new FakeRepository<TaxCalculationSnapshot>([]),
            new FakeRepository<InsuranceCalculationSnapshot>([]),
            new FakeOvertimeSnapshotProvider(),
            new FakeUnitOfWork(),
            new PayrollMapper());

        var calculateResult = await calculateHandler.Handle(
            new CalculatePayrollRunCommand(payrollPeriodId, employeeId, true, "test"),
            CancellationToken.None);

        Assert.True(calculateResult.IsT0);
        Assert.Single(payrollRuns);
        Assert.Equal(200, payrollRuns[0].BasePayAmount);
        Assert.Equal(2, payrollRuns[0].PayrollItems.Single().PaidWorkingDays);
        Assert.Equal(summaries.Single().Id, payrollRuns[0].PayrollItems.Single().AttendanceSummaryId);
    }

    [Fact]
    public async Task CalculatePayrollRun_StandardWorkingDaysZeroOrNegative_Returns_StandardWorkingDaysInvalid()
    {
        var employeeId = new EmployeeId(Guid.NewGuid());
        var payrollPeriodId = new PayrollPeriodId(Guid.NewGuid());

        var period = new PayrollPeriod
        {
            Id = payrollPeriodId,
            PeriodCode = "202605",
            StatusCode = "Draft",
            StartDate = new DateOnly(2026, 5, 1),
            EndDate = new DateOnly(2026, 5, 31),
            StandardWorkingDays = 0, // Invalid
            CreatedBy = "test",
            UpdatedBy = "test"
        };

        var handler = new CalculatePayrollRunHandler(
            new FakeRepository<PayrollPeriod>([period]),
            new FakeRepository<PayrollRun>([]),
            new FakeRepository<Employee>([CreateEmployee(employeeId)]),
            new FakeRepository<EmployeeSalary>([CreateSalary(employeeId, 3000, SalaryType.Monthly)]),
            new FakeRepository<EmployeeAllowance>([]),
            new FakeRepository<AttendancePeriod>([]),
            new FakeRepository<AttendanceSummary>([]),
            new FakeRepository<TaxCalculationSnapshot>([]),
            new FakeRepository<InsuranceCalculationSnapshot>([]),
            new FakeOvertimeSnapshotProvider(),
            new FakeUnitOfWork(),
            new PayrollMapper()
        );

        var result = await handler.Handle(
            new CalculatePayrollRunCommand(payrollPeriodId, employeeId, true, "system"),
            CancellationToken.None
        );

        Assert.True(result.IsT1);
        Assert.Equal(HrBusinessErrorCodes.PayrollStandardWorkingDaysInvalid, result.AsT1.Code);
    }

    [Fact]
    public async Task CalculatePayrollRun_MonthlySalaryFormula_IsCorrect()
    {
        var employeeId = new EmployeeId(Guid.NewGuid());
        var payrollPeriodId = new PayrollPeriodId(Guid.NewGuid());
        var attendancePeriodId = new AttendancePeriodId(Guid.NewGuid());
        var summaryId = new AttendanceSummaryId(Guid.NewGuid());

        var period = new PayrollPeriod
        {
            Id = payrollPeriodId,
            PeriodCode = "202605",
            StatusCode = "Draft",
            StartDate = new DateOnly(2026, 5, 1),
            EndDate = new DateOnly(2026, 5, 31),
            StandardWorkingDays = 22,
            CreatedBy = "test",
            UpdatedBy = "test"
        };
        typeof(PayrollPeriod).GetProperty(nameof(PayrollPeriod.AttendancePeriodId))
            ?.SetValue(period, attendancePeriodId);

        var attPeriod = new AttendancePeriod
        {
            Id = attendancePeriodId,
            PeriodCode = "202605",
            StatusCode = "Locked",
            StartDate = new DateOnly(2026, 5, 1),
            EndDate = new DateOnly(2026, 5, 31),
            CreatedBy = "test",
            UpdatedBy = "test"
        };

        var summary = new AttendanceSummary
        {
            Id = summaryId,
            AttendancePeriodId = attendancePeriodId,
            EmployeeId = employeeId,
            PaidWorkingDays = 20,
            PaidLeaveDays = 1,
            UnpaidLeaveDays = 1,
            CreatedBy = "test",
            UpdatedBy = "test"
        };

        var payrollRuns = new List<PayrollRun>();
        var handler = new CalculatePayrollRunHandler(
            new FakeRepository<PayrollPeriod>([period]),
            new FakeRepository<PayrollRun>(payrollRuns),
            new FakeRepository<Employee>([CreateEmployee(employeeId)]),
            new FakeRepository<EmployeeSalary>([CreateSalary(employeeId, 3300, SalaryType.Monthly)]),
            new FakeRepository<EmployeeAllowance>([]),
            new FakeRepository<AttendancePeriod>([attPeriod]),
            new FakeRepository<AttendanceSummary>([summary]),
            new FakeRepository<TaxCalculationSnapshot>([]),
            new FakeRepository<InsuranceCalculationSnapshot>([]),
            new FakeOvertimeSnapshotProvider(),
            new FakeUnitOfWork(),
            new PayrollMapper()
        );

        var result = await handler.Handle(
            new CalculatePayrollRunCommand(payrollPeriodId, employeeId, true, "system"),
            CancellationToken.None
        );

        Assert.True(result.IsT0);
        var payrollRun = payrollRuns.Single();

        // Standard days = 22. Monthly Salary = 3300.
        // Daily rate = 3300 / 22 = 150.
        // Worked = 20, Paid leave = 1. Total paid = 21.
        // Base pay = 150 * 21 = 3150.
        Assert.Equal(150, payrollRun.DailyRate);
        Assert.Equal(3150, payrollRun.BasePayAmount);
        Assert.Equal(3150, payrollRun.NetAmount);

        var basePayItem = payrollRun.PayrollItems.Single(x => x.ItemCode == "BASE_SALARY");
        Assert.Equal(3150, basePayItem.Amount);
        Assert.Equal(summaryId, basePayItem.AttendanceSummaryId);
        Assert.Equal(20, basePayItem.PaidWorkingDays);
        Assert.Equal(1, basePayItem.PaidLeaveDays);
        Assert.Equal(1, basePayItem.UnpaidLeaveDays);
        Assert.Equal(3300, basePayItem.BaseSalarySnapshot);
        Assert.Equal(150, basePayItem.DailyRateSnapshot);
        Assert.Equal(3150, basePayItem.BasePayAmount);
    }

    [Fact]
    public async Task CalculatePayrollRun_DailySalaryFormula_IsCorrect()
    {
        var employeeId = new EmployeeId(Guid.NewGuid());
        var payrollPeriodId = new PayrollPeriodId(Guid.NewGuid());
        var attendancePeriodId = new AttendancePeriodId(Guid.NewGuid());
        var summaryId = new AttendanceSummaryId(Guid.NewGuid());

        var period = new PayrollPeriod
        {
            Id = payrollPeriodId,
            PeriodCode = "202605",
            StatusCode = "Draft",
            StartDate = new DateOnly(2026, 5, 1),
            EndDate = new DateOnly(2026, 5, 31),
            StandardWorkingDays = 22,
            CreatedBy = "test",
            UpdatedBy = "test"
        };
        typeof(PayrollPeriod).GetProperty(nameof(PayrollPeriod.AttendancePeriodId))
            ?.SetValue(period, attendancePeriodId);

        var attPeriod = new AttendancePeriod
        {
            Id = attendancePeriodId,
            PeriodCode = "202605",
            StatusCode = "Locked",
            StartDate = new DateOnly(2026, 5, 1),
            EndDate = new DateOnly(2026, 5, 31),
            CreatedBy = "test",
            UpdatedBy = "test"
        };

        var summary = new AttendanceSummary
        {
            Id = summaryId,
            AttendancePeriodId = attendancePeriodId,
            EmployeeId = employeeId,
            PaidWorkingDays = 15,
            PaidLeaveDays = 2,
            UnpaidLeaveDays = 0,
            CreatedBy = "test",
            UpdatedBy = "test"
        };

        var payrollRuns = new List<PayrollRun>();
        var handler = new CalculatePayrollRunHandler(
            new FakeRepository<PayrollPeriod>([period]),
            new FakeRepository<PayrollRun>(payrollRuns),
            new FakeRepository<Employee>([CreateEmployee(employeeId)]),
            new FakeRepository<EmployeeSalary>([CreateSalary(employeeId, 120, SalaryType.Daily)]),
            new FakeRepository<EmployeeAllowance>([]),
            new FakeRepository<AttendancePeriod>([attPeriod]),
            new FakeRepository<AttendanceSummary>([summary]),
            new FakeRepository<TaxCalculationSnapshot>([]),
            new FakeRepository<InsuranceCalculationSnapshot>([]),
            new FakeOvertimeSnapshotProvider(),
            new FakeUnitOfWork(),
            new PayrollMapper()
        );

        var result = await handler.Handle(
            new CalculatePayrollRunCommand(payrollPeriodId, employeeId, true, "system"),
            CancellationToken.None
        );

        Assert.True(result.IsT0);
        var payrollRun = payrollRuns.Single();

        // Daily rate = 120.
        // Paid days = 15 + 2 = 17.
        // Base pay = 120 * 17 = 2040.
        Assert.Equal(120, payrollRun.DailyRate);
        Assert.Equal(2040, payrollRun.BasePayAmount);
        Assert.Equal(2040, payrollRun.NetAmount);

        var basePayItem = payrollRun.PayrollItems.Single(x => x.ItemCode == "BASE_SALARY");
        Assert.Equal(2040, basePayItem.Amount);
        Assert.Equal(summaryId, basePayItem.AttendanceSummaryId);
        Assert.Equal(15, basePayItem.PaidWorkingDays);
        Assert.Equal(2, basePayItem.PaidLeaveDays);
        Assert.Equal(0, basePayItem.UnpaidLeaveDays);
        Assert.Equal(120, basePayItem.BaseSalarySnapshot);
        Assert.Equal(120, basePayItem.DailyRateSnapshot);
        Assert.Equal(2040, basePayItem.BasePayAmount);
    }

    [Fact]
    public async Task RecalculatePayrollRun_DoesNotDuplicate_PayrollItems()
    {
        var employeeId = new EmployeeId(Guid.NewGuid());
        var payrollPeriodId = new PayrollPeriodId(Guid.NewGuid());
        var attendancePeriodId = new AttendancePeriodId(Guid.NewGuid());
        var summaryId = new AttendanceSummaryId(Guid.NewGuid());

        var period = new PayrollPeriod
        {
            Id = payrollPeriodId,
            PeriodCode = "202605",
            StatusCode = "Draft",
            StartDate = new DateOnly(2026, 5, 1),
            EndDate = new DateOnly(2026, 5, 31),
            StandardWorkingDays = 22,
            CreatedBy = "test",
            UpdatedBy = "test"
        };
        typeof(PayrollPeriod).GetProperty(nameof(PayrollPeriod.AttendancePeriodId))
            ?.SetValue(period, attendancePeriodId);

        var attPeriod = new AttendancePeriod
        {
            Id = attendancePeriodId,
            PeriodCode = "202605",
            StatusCode = "Locked",
            StartDate = new DateOnly(2026, 5, 1),
            EndDate = new DateOnly(2026, 5, 31),
            CreatedBy = "test",
            UpdatedBy = "test"
        };

        var summary = new AttendanceSummary
        {
            Id = summaryId,
            AttendancePeriodId = attendancePeriodId,
            EmployeeId = employeeId,
            PaidWorkingDays = 20,
            PaidLeaveDays = 0,
            UnpaidLeaveDays = 2,
            CreatedBy = "test",
            UpdatedBy = "test"
        };

        var runId = new PayrollRunId(Guid.NewGuid());
        var payrollRun = new PayrollRun
        {
            Id = runId,
            PayrollPeriodId = payrollPeriodId,
            EmployeeId = employeeId,
            EmployeeCode = "EMP-TEST",
            EmployeeName = "John Doe",
            BaseSalary = 2200,
            CurrencyCode = "USD",
            PayScheduleType = "Monthly",
            StandardWorkingDays = 22,
            PaidWorkingDays = 10,
            UnpaidLeaveDays = 12,
            DailyRate = 100,
            BasePayAmount = 1000,
            TotalAllowanceAmount = 0,
            GrossAmount = 1000,
            TotalDeductionAmount = 0,
            NetAmount = 1000,
            CalculatedAt = DateTime.UtcNow,
            CalculatedBy = "system"
        };

        // Seed with existing item
        var initialItem = new PayrollItem
        {
            Id = new PayrollItemId(Guid.NewGuid()),
            PayrollRunId = runId,
            ItemCode = "BASE_SALARY",
            ItemName = "Base Salary",
            ItemTypeCode = PayrollItemType.BasePay,
            Amount = 1000,
            CurrencyCode = "USD",
            PaidWorkingDays = 10,
            PaidLeaveDays = 0,
            UnpaidLeaveDays = 12,
            BaseSalarySnapshot = 2200,
            DailyRateSnapshot = 100,
            BasePayAmount = 1000
        };
        payrollRun.PayrollItems.Add(initialItem);

        var itemsList = new List<PayrollItem> { initialItem };

        var handler = new RecalculatePayrollRunHandler(
            new FakeRepository<PayrollPeriod>([period]),
            new FakeRepository<PayrollRun>([payrollRun]),
            new FakeRepository<PayrollItem>(itemsList),
            new FakeRepository<Employee>([CreateEmployee(employeeId)]),
            new FakeRepository<EmployeeSalary>([CreateSalary(employeeId, 2200, SalaryType.Monthly)]),
            new FakeRepository<EmployeeAllowance>([]),
            new FakeRepository<AttendancePeriod>([attPeriod]),
            new FakeRepository<AttendanceSummary>([summary]),
            new FakeUnitOfWork(),
            new PayrollMapper()
        );

        var result = await handler.Handle(
            new RecalculatePayrollRunCommand(runId, true, "system"),
            CancellationToken.None
        );

        Assert.True(result.IsT0);
        // Verify that the initial item was removed, and only the new recalculated item exists on payrollRun.
        Assert.Single(payrollRun.PayrollItems);
        Assert.NotEqual(initialItem.Id, payrollRun.PayrollItems.First().Id);
        Assert.Equal(2000, payrollRun.BasePayAmount);
        Assert.Equal(20, payrollRun.PayrollItems.First().PaidWorkingDays);
    }

    private sealed class FakeOvertimeSnapshotProvider : IOvertimeSnapshotProvider
    {
        public Task<IEnumerable<OvertimeRequestResponse>> GetApprovedOvertimeRequestsAsync(
            DateTime fromDate, DateTime toDate, CancellationToken cancellationToken)
        {
            return Task.FromResult(Enumerable.Empty<OvertimeRequestResponse>());
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<OneOf<None, Exception>> SaveChangesAsync(CancellationToken token = default)
        {
            return Task.FromResult<OneOf<None, Exception>>(None.Value);
        }
    }

    private sealed class FailingUnitOfWork : IUnitOfWork
    {
        public Task<OneOf<None, Exception>> SaveChangesAsync(CancellationToken token = default)
        {
            return Task.FromResult<OneOf<None, Exception>>(new InvalidOperationException("Save failed"));
        }
    }

    private sealed class FakeRepository<T>(List<T> initialItems) : ISqlRepository<T>
        where T : class
    {
        private readonly List<T> _items = initialItems;

        public IQueryable<T> GetQueryable(Expression<Func<T, bool>> conditionExpression = null)
        {
            var queryable = _items.AsQueryable();
            if (conditionExpression is not null)
                queryable = queryable.Where(conditionExpression);
            return new TestAsyncEnumerable<T>(queryable);
        }

        public IQueryable<T> GetQueryableFromRawQuery(string sql, params object[] parameters) => GetQueryable();

        public Task<T> GetFirstByConditionAsync(
            Expression<Func<T, bool>> conditionExpression = null,
            Func<IQueryable<T>, IQueryable<T>> specialAction = null,
            CancellationToken token = default)
        {
            IQueryable<T> queryable = GetQueryable(conditionExpression);
            if (specialAction is not null)
                queryable = specialAction(queryable);
            return Task.FromResult(queryable.FirstOrDefault());
        }

        public Task<bool> ExistByConditionAsync(Expression<Func<T, bool>> conditionExpression = null, CancellationToken token = default)
        {
            return Task.FromResult(GetQueryable(conditionExpression).Any());
        }

        public Task<List<T>> GetManyByConditionAsync(
            Expression<Func<T, bool>> conditionExpression = null,
            Func<IQueryable<T>, IQueryable<T>> specialAction = null,
            CancellationToken token = default)
        {
            IQueryable<T> queryable = GetQueryable(conditionExpression);
            if (specialAction is not null)
                queryable = specialAction(queryable);
            return Task.FromResult(queryable.ToList());
        }

        public Task<Pagination<T>> GetManyByConditionWithPaginationAsync(Expression<Func<T, bool>> conditionExpression = null, Func<IQueryable<T>, IQueryable<T>> specialAction = null, CancellationToken token = default) => throw new NotSupportedException();
        public Task<long> CountByConditionAsync(Expression<Func<T, bool>> conditionExpression = null, Func<IQueryable<T>, IQueryable<T>> specialAction = null, CancellationToken token = default) => Task.FromResult((long)GetQueryable(conditionExpression).Count());

        public Task<OneOf<T, Exception>> CreateOneAsync(T item, CancellationToken token = default)
        {
            _items.Add(item);
            return Task.FromResult<OneOf<T, Exception>>(item);
        }

        public Task<OneOf<None, Exception>> CreateManyAsync(List<T> items, CancellationToken token = default)
        {
            _items.AddRange(items);
            return Task.FromResult<OneOf<None, Exception>>(None.Value);
        }

        public Task<OneOf<None, Exception>> RemoveOneAsync(OneOf<T, Expression<Func<T, bool>>> itemOrFilter, CancellationToken token = default)
        {
            if (itemOrFilter.IsT0)
            {
                _items.Remove(itemOrFilter.AsT0);
            }
            return Task.FromResult<OneOf<None, Exception>>(None.Value);
        }

        public Task<OneOf<None, Exception>> RemoveManyAsync(OneOf<List<T>, Expression<Func<T, bool>>> itemsOrFilter, CancellationToken token = default) => throw new NotSupportedException();
    }

    private sealed class TestAsyncQueryProvider<TElement>(IQueryProvider inner) : IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TElement>(expression);
        public IQueryable<TElement1> CreateQuery<TElement1>(Expression expression) => new TestAsyncEnumerable<TElement1>(expression);
        public object Execute(Expression expression) => inner.Execute(expression);
        public TResult Execute<TResult>(Expression expression) => inner.Execute<TResult>(expression);
        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var expectedResultType = typeof(TResult).GetGenericArguments().FirstOrDefault();
            if (expectedResultType is null)
                return Execute<TResult>(expression);

            var executeMethod = typeof(IQueryProvider)
                .GetMethods()
                .Single(x => x.Name == nameof(IQueryProvider.Execute) && x.IsGenericMethod)
                .MakeGenericMethod(expectedResultType);
            var result = executeMethod.Invoke(inner, [expression]);
            return (TResult)typeof(Task)
                .GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(expectedResultType)
                .Invoke(null, [result])!;
        }
    }

    private sealed class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable)
        {
        }

        public TestAsyncEnumerable(Expression expression) : base(expression)
        {
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    private sealed class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
    {
        public T Current => inner.Current;
        public ValueTask DisposeAsync()
        {
            inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync() => new(inner.MoveNext());
    }
}
