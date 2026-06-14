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

        var overtimeHours = await query
            .Select(x => new
            {
                x.Status,
                StartTicks = (long)x.StartTime.Ticks,
                EndTicks = (long)x.EndTime.Ticks
            })
            .ToListAsync(cancellationToken);

        var hoursData = overtimeHours
            .Select(x => new
            {
                x.Status,
                Hours = new TimeSpan(x.EndTicks - x.StartTicks).TotalHours
            })
            .ToList();

        var totalHours = hoursData.Sum(x => x.Hours);
        var avgHours = hoursData.Count > 0 ? hoursData.Average(x => x.Hours) : 0;

        return new OvertimeAnalyticsResponse(
            Math.Round((decimal)totalHours, 2),
            Math.Round((decimal)avgHours, 2),
            approvedCount,
            rejectedCount
        );
    }
}
