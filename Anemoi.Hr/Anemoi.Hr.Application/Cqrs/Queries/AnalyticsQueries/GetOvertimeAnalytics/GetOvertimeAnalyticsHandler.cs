using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Overtime;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetOvertimeAnalytics;

public sealed class GetOvertimeAnalyticsHandler(
    ISqlRepository<OvertimeRequest> overtimeRepository)
    : IQueryHandler<GetOvertimeAnalyticsQuery, OvertimeAnalyticsResponse>
{
    public async Task<OvertimeAnalyticsResponse> Handle(
        GetOvertimeAnalyticsQuery request,
        CancellationToken cancellationToken)
    {
        var query = overtimeRepository.GetQueryable().AsNoTracking();

        var approvedCount = await query.CountAsync(x => x.Status == OvertimeStatusCode.Approved, cancellationToken);
        var rejectedCount = await query.CountAsync(x => x.Status == OvertimeStatusCode.Rejected, cancellationToken);

        var hourStats = await query
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalTicks = g.Sum(x => x.EndTime.Ticks - x.StartTime.Ticks),
                AvgTicks = g.Average(x => (double)(x.EndTime.Ticks - x.StartTime.Ticks))
            })
            .FirstOrDefaultAsync(cancellationToken);

        double totalHours = 0;
        double avgHours = 0;
        if (hourStats is not null)
        {
            totalHours = new TimeSpan((long)hourStats.TotalTicks).TotalHours;
            avgHours = new TimeSpan((long)hourStats.AvgTicks).TotalHours;
        }

        return new OvertimeAnalyticsResponse(
            Math.Round((decimal)totalHours, 2),
            Math.Round((decimal)avgHours, 2),
            approvedCount,
            rejectedCount
        );
    }
}
