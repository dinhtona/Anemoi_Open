using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Compensation;
using Anemoi.Hr.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.CompensationQueries.GetCompensationDashboard;

public sealed class GetCompensationDashboardHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<EmployeeSalary> salaryRepository,
    ISqlRepository<EmployeeAllowance> allowanceRepository,
    ISqlRepository<SalaryValidationBypassLog> bypassLogRepository,
    HrSettings hrSettings,
    CompensationMapper mapper)
    : IQueryHandler<GetCompensationDashboardQuery, CompensationDashboardResponse>
{
    public async Task<CompensationDashboardResponse> Handle(
        GetCompensationDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var today = GetBusinessToday();

        var activeSalaries = await salaryRepository.GetQueryable()
            .AsNoTracking()
            .Where(x => x.EffectiveFrom <= today && (x.EffectiveTo == null || x.EffectiveTo >= today))
            .ToListAsync(cancellationToken);

        var activeAllowances = await allowanceRepository.GetQueryable()
            .AsNoTracking()
            .Include(x => x.AllowanceType)
            .Where(x => x.EffectiveFrom <= today && (x.EffectiveTo == null || x.EffectiveTo >= today))
            .ToListAsync(cancellationToken);

        var recentBypasses = await bypassLogRepository.GetQueryable()
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        // Find employees without active salaries
        var salaryEmployeeIds = activeSalaries.Select(s => s.EmployeeId.Value).Distinct().ToList();
        var missingSalaryEmployeeIds = salaryEmployeeIds.Count > 0
            ? await employeeRepository.GetQueryable()
                .Where(e => e.EmploymentStatusCode == EmploymentStatusCode.Active
                    && !salaryEmployeeIds.Contains(e.Id.Value))
                .OrderBy(e => e.FullName)
                .Take(50)
                .Select(e => e.Id.Value.ToString())
                .ToListAsync(cancellationToken)
            : await employeeRepository.GetQueryable()
                .Where(e => e.EmploymentStatusCode == EmploymentStatusCode.Active)
                .OrderBy(e => e.FullName)
                .Take(50)
                .Select(e => e.Id.Value.ToString())
                .ToListAsync(cancellationToken);

        var costByGrade = new Dictionary<string, decimal>();
        var projectionByCurrency = new Dictionary<string, PayrollProjectionByCurrencyResponse>(StringComparer.OrdinalIgnoreCase);

        foreach (var salary in activeSalaries)
        {
            var monthlySalaryCost = salary.SalaryType == SalaryType.Monthly
                ? salary.BaseSalary
                : salary.BaseSalary * 21.75m;

            if (!projectionByCurrency.TryGetValue(salary.Currency, out var projection))
            {
                projection = new PayrollProjectionByCurrencyResponse { Currency = salary.Currency };
                projectionByCurrency[salary.Currency] = projection;
            }

            if (salary.SalaryType == SalaryType.Monthly)
                projection.MonthlySalaryTotal += monthlySalaryCost;
            else
                projection.DailySalaryTotal += monthlySalaryCost;

            var gradeCode = salary.GradeCodeSnapshot ?? "Unknown";
            if (!costByGrade.TryGetValue(gradeCode, out var currentCost))
            {
                costByGrade[gradeCode] = 0;
            }
            costByGrade[gradeCode] += monthlySalaryCost;
        }

        foreach (var allowance in activeAllowances)
        {
            if (!projectionByCurrency.TryGetValue(allowance.Currency, out var projection))
            {
                projection = new PayrollProjectionByCurrencyResponse { Currency = allowance.Currency };
                projectionByCurrency[allowance.Currency] = projection;
            }

            projection.AllowanceTotal += allowance.Amount;
        }

        foreach (var projection in projectionByCurrency.Values)
        {
            projection.Total = projection.MonthlySalaryTotal + projection.DailySalaryTotal + projection.AllowanceTotal;
        }

        var allowanceCostByType = activeAllowances
            .GroupBy(x => new
            {
                AllowanceTypeCode = x.AllowanceType?.Code ?? x.AllowanceTypeId.Value.ToString(),
                x.Currency
            })
            .Select(x => new AllowanceCostByTypeResponse
            {
                AllowanceTypeCode = x.Key.AllowanceTypeCode,
                Currency = x.Key.Currency,
                Total = x.Sum(a => a.Amount)
            })
            .OrderBy(x => x.AllowanceTypeCode)
            .ThenBy(x => x.Currency)
            .ToList();

        return new CompensationDashboardResponse
        {
            PayrollProjectionByCurrency = projectionByCurrency.Values.OrderBy(x => x.Currency).ToList(),
            CostByGrade = costByGrade,
            AllowanceCostByType = allowanceCostByType,
            MissingSalaryEmployeeIds = missingSalaryEmployeeIds,
            RecentBypasses = recentBypasses.Select(mapper.ToSalaryValidationBypassLogResponse).ToList()
        };
    }

    private DateOnly GetBusinessToday()
    {
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(hrSettings.BusinessTimeZone);
            var localTime = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
            return DateOnly.FromDateTime(localTime.DateTime);
        }
        catch
        {
            return DateOnly.FromDateTime(DateTime.UtcNow);
        }
    }
}
