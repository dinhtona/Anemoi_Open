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
    ISqlRepository<PayrollRun> payrollRunRepository)
    : IQueryHandler<GetPayrollAnalyticsQuery, PayrollAnalyticsResponse>
{
    public async Task<PayrollAnalyticsResponse> Handle(
        GetPayrollAnalyticsQuery request,
        CancellationToken cancellationToken)
    {
        var finalizedRuns = payrollRunRepository.GetQueryable()
            .Where(x => x.Status == PayrollRunStatus.Finalized)
            .AsNoTracking();

        var totals = await finalizedRuns
            .GroupBy(x => 1)
            .Select(g => new
            {
                TotalPayrollCost = g.Sum(x => x.NetAmount),
                AverageSalary = g.Average(x => x.NetAmount),
                HighestSalary = g.Max(x => x.NetAmount),
                LowestSalary = g.Min(x => x.NetAmount),
                EmployeeCount = g.Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (totals is null)
            return new PayrollAnalyticsResponse(0, 0, 0, 0, 0);

        return new PayrollAnalyticsResponse(
            totals.TotalPayrollCost,
            totals.AverageSalary,
            totals.HighestSalary,
            totals.LowestSalary,
            totals.EmployeeCount
        );
    }
}
