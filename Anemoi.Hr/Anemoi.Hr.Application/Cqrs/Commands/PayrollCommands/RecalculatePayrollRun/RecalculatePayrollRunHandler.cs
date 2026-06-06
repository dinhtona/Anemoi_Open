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
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.RecalculatePayrollRun;

public sealed class RecalculatePayrollRunHandler(
    ISqlRepository<PayrollPeriod> payrollPeriodRepository,
    ISqlRepository<PayrollRun> payrollRunRepository,
    ISqlRepository<PayrollItem> payrollItemRepository,
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<EmployeeSalary> employeeSalaryRepository,
    ISqlRepository<EmployeeAllowance> employeeAllowanceRepository,
    IUnitOfWork unitOfWork,
    PayrollMapper mapper)
    : ICommandHandler<RecalculatePayrollRunCommand, OneOf<PayrollRunDetailResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<PayrollRunDetailResponse, ErrorDetailResponse>> Handle(
        RecalculatePayrollRunCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch existing PayrollRun (including items)
        var payrollRun = await payrollRunRepository.GetFirstByConditionAsync(
            x => x.Id == request.PayrollRunId,
            q => q.Include(r => r.PayrollItems),
            cancellationToken);

        if (payrollRun is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollRunNotFound);

        // 2. Fetch Payroll Period
        var period = await payrollPeriodRepository.GetFirstByConditionAsync(
            x => x.Id == payrollRun.PayrollPeriodId,
            null,
            cancellationToken);

        if (period is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollPeriodNotFound);

        // 3. Reject if payroll period is locked (only Draft allowed)
        if (period.StatusCode != "Draft")
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollPeriodLocked);

        // 4. Fetch Employee
        var employee = await employeeRepository.GetFirstByConditionAsync(
            x => x.Id == payrollRun.EmployeeId,
            null,
            cancellationToken);

        if (employee is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        // 5. Read active employee salary snapshot on period.EndDate
        var activeSalary = await employeeSalaryRepository.GetQueryable()
            .Where(x => x.EmployeeId == payrollRun.EmployeeId &&
                        x.EffectiveFrom <= period.EndDate &&
                        (x.EffectiveTo == null || x.EffectiveTo >= period.EndDate))
            .FirstOrDefaultAsync(cancellationToken);

        if (activeSalary is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeSalaryNotFound);

        // 6. Validate working days count
        if (request.PaidWorkingDays + request.UnpaidLeaveDays > period.StandardWorkingDays)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollInvalidWorkingDays);

        // 7. Remove existing PayrollItems
        foreach (var item in payrollRun.PayrollItems.ToList())
        {
            await payrollItemRepository.RemoveOneAsync(item, cancellationToken);
        }
        payrollRun.PayrollItems.Clear();

        // 8. Recalculate details
        var baseSalary = activeSalary.BaseSalary;
        var currencyCode = activeSalary.Currency;
        var payScheduleType = activeSalary.SalaryType.ToString();

        decimal dailyRate;
        if (activeSalary.SalaryType == SalaryType.Monthly)
        {
            dailyRate = Math.Round(baseSalary / period.StandardWorkingDays, 4, MidpointRounding.AwayFromZero);
        }
        else // Daily
        {
            dailyRate = Math.Round(baseSalary, 4, MidpointRounding.AwayFromZero);
        }

        var basePayAmount = Math.Round(dailyRate * request.PaidWorkingDays, 2, MidpointRounding.AwayFromZero);

        // Fetch active allowances
        var activeAllowances = await employeeAllowanceRepository.GetQueryable()
            .Include(x => x.AllowanceType)
            .Where(x => x.EmployeeId == payrollRun.EmployeeId &&
                        x.EffectiveFrom <= period.EndDate &&
                        (x.EffectiveTo == null || x.EffectiveTo >= period.EndDate))
            .ToListAsync(cancellationToken);

        var totalAllowanceAmount = activeAllowances.Sum(x => x.Amount);
        var grossAmount = Math.Round(basePayAmount + totalAllowanceAmount, 2, MidpointRounding.AwayFromZero);
        var totalDeductionAmount = 0m;
        var netAmount = Math.Round(grossAmount - totalDeductionAmount, 2, MidpointRounding.AwayFromZero);

        // 9. Update existing PayrollRun details in-place
        payrollRun.BaseSalary = baseSalary;
        payrollRun.CurrencyCode = currencyCode;
        payrollRun.PayScheduleType = payScheduleType;
        payrollRun.StandardWorkingDays = period.StandardWorkingDays;
        payrollRun.PaidWorkingDays = request.PaidWorkingDays;
        payrollRun.UnpaidLeaveDays = request.UnpaidLeaveDays;
        payrollRun.DailyRate = dailyRate;
        payrollRun.BasePayAmount = basePayAmount;
        payrollRun.TotalAllowanceAmount = totalAllowanceAmount;
        payrollRun.GrossAmount = grossAmount;
        payrollRun.TotalDeductionAmount = totalDeductionAmount;
        payrollRun.NetAmount = netAmount;
        payrollRun.CalculatedAt = DateTime.UtcNow;
        payrollRun.CalculatedBy = request.CalculatedBy ?? "system";

        // 10. Recreate PayrollItems
        // Base Pay Item
        var basePayItem = new PayrollItem
        {
            Id = new PayrollItemId(IdGenerator.NextGuid()),
            PayrollRunId = payrollRun.Id,
            ItemCode = "BASE_SALARY",
            ItemName = "Base Salary",
            ItemTypeCode = PayrollItemType.BasePay,
            Amount = basePayAmount,
            CurrencyCode = currencyCode
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
                    CurrencyCode = allowance.Currency
                };
                payrollRun.PayrollItems.Add(allowanceItem);
            }
        }

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
