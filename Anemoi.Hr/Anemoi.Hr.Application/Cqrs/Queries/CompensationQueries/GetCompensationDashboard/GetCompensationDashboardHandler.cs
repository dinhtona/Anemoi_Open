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

        var employees = await employeeRepository.GetQueryable().ToListAsync(cancellationToken);

        var activeSalaries = await salaryRepository.GetQueryable()
            .Where(x => x.EffectiveFrom <= today && (x.EffectiveTo == null || x.EffectiveTo >= today))
            .ToListAsync(cancellationToken);

        var activeAllowances = await allowanceRepository.GetQueryable()
            .Where(x => x.EffectiveFrom <= today && (x.EffectiveTo == null || x.EffectiveTo >= today))
            .ToListAsync(cancellationToken);

        var recentBypasses = await bypassLogRepository.GetQueryable()
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        // Compute payroll projection
        decimal totalMonthlyPayroll = 0;
        var costByGrade = new Dictionary<string, decimal>();

        foreach (var salary in activeSalaries)
        {
            var monthlySalaryCost = salary.SalaryType == SalaryType.Monthly
                ? salary.BaseSalary
                : salary.BaseSalary * 21.75m;

            totalMonthlyPayroll += monthlySalaryCost;

            var gradeCode = salary.GradeCodeSnapshot ?? "Unknown";
            if (!costByGrade.TryGetValue(gradeCode, out var currentCost))
            {
                costByGrade[gradeCode] = 0;
            }
            costByGrade[gradeCode] += monthlySalaryCost;
        }

        // Add allowances to total projection
        totalMonthlyPayroll += activeAllowances.Sum(x => x.Amount);

        // Find employees without salaries
        var employeesWithSalaryIds = activeSalaries.Select(s => s.EmployeeId).ToHashSet();
        var missingSalaryEmployeeIds = employees
            .Where(e => !employeesWithSalaryIds.Contains(e.Id))
            .Select(e => e.Id.Value.ToString())
            .ToList();

        return new CompensationDashboardResponse
        {
            TotalMonthlyPayrollProjection = totalMonthlyPayroll,
            CostByGrade = costByGrade,
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
