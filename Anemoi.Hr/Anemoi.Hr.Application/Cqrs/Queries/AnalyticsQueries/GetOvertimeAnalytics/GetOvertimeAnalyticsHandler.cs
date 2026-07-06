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

        var allDurations = await query
            .Select(x => x.EndTime.Ticks - x.StartTime.Ticks)
            .ToListAsync(cancellationToken);

        double totalHours = 0;
        double avgHours = 0;
        if (allDurations.Count > 0)
        {
            totalHours = new TimeSpan(allDurations.Sum()).TotalHours;
            avgHours = new TimeSpan((long)allDurations.Average()).TotalHours;
        }

        return new OvertimeAnalyticsResponse(
            Math.Round((decimal)totalHours, 2),
            Math.Round((decimal)avgHours, 2),
            approvedCount,
            rejectedCount
        );
    }
}
