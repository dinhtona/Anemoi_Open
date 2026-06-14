using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Compensation;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.Hr.Domain.Attendance;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.CalculatePayrollRun;

public sealed class CalculatePayrollRunHandler(
    ISqlRepository<PayrollPeriod> payrollPeriodRepository,
    ISqlRepository<PayrollRun> payrollRunRepository,
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<EmployeeSalary> employeeSalaryRepository,
    ISqlRepository<EmployeeAllowance> employeeAllowanceRepository,
    ISqlRepository<AttendancePeriod> attendancePeriodRepository,
    ISqlRepository<AttendanceSummary> attendanceSummaryRepository,
    IUnitOfWork unitOfWork,
    PayrollMapper mapper,
    ILogger<CalculatePayrollRunHandler> logger = null)
    : ICommandHandler<CalculatePayrollRunCommand, OneOf<PayrollRunDetailResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<PayrollRunDetailResponse, ErrorDetailResponse>> Handle(
        CalculatePayrollRunCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch Payroll Period
        var period = await payrollPeriodRepository.GetFirstByConditionAsync(
            x => x.Id == request.PayrollPeriodId,
            null,
            cancellationToken);

        if (period is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollPeriodNotFound);

        // 2. Reject if payroll period is locked
        if (period.StatusCode == "Locked")
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollPeriodLocked);

        // Validate Standard Working Days
        if (period.StandardWorkingDays <= 0)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollStandardWorkingDaysInvalid);

        // Validate AttendancePeriod link
        if (period.AttendancePeriodId == null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollAttendancePeriodNotLinked);

        // 3. Verify AttendancePeriod is locked
        var attendancePeriod = await attendancePeriodRepository.GetFirstByConditionAsync(
            x => x.Id == period.AttendancePeriodId,
            null,
            cancellationToken);

        if (attendancePeriod is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.AttendancePeriodNotFound);

        if (attendancePeriod.StatusCode != "Locked")
            return HrErrorResponses.Create(HrBusinessErrorCodes.AttendancePeriodNotLocked);

        // 4. Reject if payroll run already exists
        var runExists = await payrollRunRepository.ExistByConditionAsync(
            x => x.PayrollPeriodId == request.PayrollPeriodId && x.EmployeeId == request.EmployeeId,
            cancellationToken);

        if (runExists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollRunAlreadyExists);

        // 5. Fetch Employee
        var employee = await employeeRepository.GetFirstByConditionAsync(
            x => x.Id == request.EmployeeId,
            null,
            cancellationToken);

        if (employee is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        // 6. Read active employee salary snapshot
        var activeSalary = await employeeSalaryRepository.GetQueryable()
            .Where(x => x.EmployeeId == request.EmployeeId &&
                        x.EffectiveFrom <= period.EndDate &&
                        (x.EffectiveTo == null || x.EffectiveTo >= period.EndDate))
            .FirstOrDefaultAsync(cancellationToken);

        if (activeSalary is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeSalaryNotFound);

        // 7. Retrieve AttendanceSummary
        var summary = await attendanceSummaryRepository.GetFirstByConditionAsync(
            x => x.AttendancePeriodId == period.AttendancePeriodId && x.EmployeeId == request.EmployeeId,
            null,
            cancellationToken);

        if (summary is null)
        {
            (logger ?? NullLogger<CalculatePayrollRunHandler>.Instance).LogWarning(
                "Attendance summary not found for payroll calculation. PayrollPeriodId: {PayrollPeriodId}, AttendancePeriodId: {AttendancePeriodId}, EmployeeId: {EmployeeId}",
                request.PayrollPeriodId.Value,
                period.AttendancePeriodId.Value,
                request.EmployeeId.Value);
            return HrErrorResponses.Create(HrBusinessErrorCodes.AttendanceSummaryNotFound);
        }

        // 8. Calculate details
        var baseSalary = activeSalary.BaseSalary;
        var currencyCode = activeSalary.Currency;
        var payScheduleType = activeSalary.SalaryType.ToString();

        decimal dailyRate;
        decimal baseSalarySnapshot;

        if (activeSalary.SalaryType == SalaryType.Monthly)
        {
            baseSalarySnapshot = baseSalary; // MonthlyContractSalary
            dailyRate = Math.Round(baseSalarySnapshot / period.StandardWorkingDays, 4, MidpointRounding.AwayFromZero);
        }
        else // Daily
        {
            baseSalarySnapshot = baseSalary; // DailySalaryRate
            dailyRate = Math.Round(baseSalary, 4, MidpointRounding.AwayFromZero);
        }

        var basePayAmount = Math.Round(dailyRate * (summary.PaidWorkingDays + summary.PaidLeaveDays), 2, MidpointRounding.AwayFromZero);

        // Fetch active allowances
        var activeAllowances = await employeeAllowanceRepository.GetQueryable()
            .Include(x => x.AllowanceType)
            .Where(x => x.EmployeeId == request.EmployeeId &&
                        x.EffectiveFrom <= period.EndDate &&
                        (x.EffectiveTo == null || x.EffectiveTo >= period.EndDate))
            .ToListAsync(cancellationToken);

        var totalAllowanceAmount = activeAllowances.Sum(x => x.Amount);
        var grossAmount = Math.Round(basePayAmount + totalAllowanceAmount, 2, MidpointRounding.AwayFromZero);
        var totalDeductionAmount = 0m;
        var netAmount = Math.Round(grossAmount - totalDeductionAmount, 2, MidpointRounding.AwayFromZero);

        // 9. Create PayrollRun
        var payrollRun = new PayrollRun
        {
            Id = new PayrollRunId(IdGenerator.NextGuid()),
            PayrollPeriodId = request.PayrollPeriodId,
            EmployeeId = request.EmployeeId,
            EmployeeCode = employee.EmployeeCode,
            EmployeeName = employee.FullName,
            BaseSalary = baseSalary,
            CurrencyCode = currencyCode,
            PayScheduleType = payScheduleType,
            StandardWorkingDays = period.StandardWorkingDays,
            PaidWorkingDays = summary.PaidWorkingDays,
            UnpaidLeaveDays = summary.UnpaidLeaveDays,
            DailyRate = dailyRate,
            BasePayAmount = basePayAmount,
            TotalAllowanceAmount = totalAllowanceAmount,
            GrossAmount = grossAmount,
            TotalDeductionAmount = totalDeductionAmount,
            NetAmount = netAmount,
            CalculatedAt = DateTime.UtcNow,
            CalculatedBy = request.CalculatedBy ?? "system"
        };

        // 10. Add PayrollItems
        // Base Pay Item
        var basePayItem = new PayrollItem
        {
            Id = new PayrollItemId(IdGenerator.NextGuid()),
            PayrollRunId = payrollRun.Id,
            ItemCode = "BASE_SALARY",
            ItemName = "Base Salary",
            ItemTypeCode = PayrollItemType.BasePay,
            Amount = basePayAmount,
            CurrencyCode = currencyCode,
            AttendanceSummaryId = summary.Id,
            PaidWorkingDays = summary.PaidWorkingDays,
            PaidLeaveDays = summary.PaidLeaveDays,
            UnpaidLeaveDays = summary.UnpaidLeaveDays,
            BaseSalarySnapshot = baseSalarySnapshot,
            DailyRateSnapshot = dailyRate,
            BasePayAmount = basePayAmount
        };
        payrollRun.PayrollItems.Add(basePayItem);

        // Allowance Items (only add if amount > 0)
        foreach (var allowance in activeAllowances)
        {
            if (allowance.Amount > 0)
            {
                var allowanceItem = new PayrollItem
                {
                    Id = new PayrollItemId(IdGenerator.NextGuid()),
                    PayrollRunId = payrollRun.Id,
                    ItemCode = allowance.AllowanceType?.Code ?? "ALLOWANCE",
                    ItemName = allowance.AllowanceType?.Name ?? "Allowance",
                    ItemTypeCode = PayrollItemType.Allowance,
                    Amount = allowance.Amount,
                    CurrencyCode = allowance.Currency,
                    PaidWorkingDays = 0,
                    PaidLeaveDays = 0,
                    UnpaidLeaveDays = 0,
                    BaseSalarySnapshot = allowance.Amount,
                    DailyRateSnapshot = allowance.Amount,
                    BasePayAmount = 0
                };
                payrollRun.PayrollItems.Add(allowanceItem);
            }
        }

        await payrollRunRepository.CreateOneAsync(payrollRun, cancellationToken);
        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrErrorResponses.Create("HR_PAYROLL_RUN_CONCURRENCY_CONFLICT")
                : HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");
        }

        return mapper.ToDetailResponse(payrollRun);
    }
}
