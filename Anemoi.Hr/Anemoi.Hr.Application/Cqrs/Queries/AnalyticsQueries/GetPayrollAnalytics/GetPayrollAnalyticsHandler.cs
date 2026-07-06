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

        var totalPayrollCost = await finalizedItems
            .Select(x => x.PayrollRun.NetAmount)
            .Distinct()
            .SumAsync(cancellationToken);

        var salaryStats = await finalizedItems
            .Select(x => x.BaseSalarySnapshot)
            .Distinct()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Count = g.Count(),
                Average = g.Average(),
                Max = g.Max(),
                Min = g.Min()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (salaryStats is null || salaryStats.Count == 0)
            return new PayrollAnalyticsResponse(0, 0, 0, 0, 0);

        return new PayrollAnalyticsResponse(
            totalPayrollCost,
            salaryStats.Average,
            salaryStats.Max,
            salaryStats.Min,
            salaryStats.Count
        );
    }
}
