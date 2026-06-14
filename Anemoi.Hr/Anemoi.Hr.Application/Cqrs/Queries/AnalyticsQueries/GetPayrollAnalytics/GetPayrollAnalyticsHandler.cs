using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetPayrollAnalytics;

public sealed class GetPayrollAnalyticsHandler(
    ISqlRepository<PayrollItem> payrollItemRepository)
    : IQueryHandler<GetPayrollAnalyticsQuery, PayrollAnalyticsResponse>
{
    public async Task<PayrollAnalyticsResponse> Handle(
        GetPayrollAnalyticsQuery request,
        CancellationToken cancellationToken)
    {
        var finalizedItems = payrollItemRepository.GetQueryable()
            .Where(x => x.PayrollRun.Status == PayrollRunStatus.Finalized)
            .AsNoTracking();

        var runCosts = await finalizedItems
            .Select(x => new { x.PayrollRunId, x.PayrollRun.NetAmount })
            .Distinct()
            .ToListAsync(cancellationToken);

        var totalPayrollCost = runCosts.Sum(x => x.NetAmount);

        var employeeSalaries = await finalizedItems
            .Select(x => new
            {
                EmployeeId = x.PayrollRun.EmployeeId,
                x.BaseSalarySnapshot
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        var employeeCount = employeeSalaries.Count;

        if (employeeCount == 0)
            return new PayrollAnalyticsResponse(0, 0, 0, 0, 0);

        var averageSalary = employeeSalaries.Average(x => x.BaseSalarySnapshot);
        var highestSalary = employeeSalaries.Max(x => x.BaseSalarySnapshot);
        var lowestSalary = employeeSalaries.Min(x => x.BaseSalarySnapshot);

        return new PayrollAnalyticsResponse(
            totalPayrollCost,
            averageSalary,
            highestSalary,
            lowestSalary,
            employeeCount
        );
    }
}
