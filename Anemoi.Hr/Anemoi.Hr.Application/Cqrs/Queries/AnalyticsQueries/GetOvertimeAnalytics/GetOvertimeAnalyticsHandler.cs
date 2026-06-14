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

        var totals = await query
            .GroupBy(x => 1)
            .Select(g => new
            {
                TotalOvertimeHours = g.Sum(x => (decimal)(x.EndTime - x.StartTime).TotalHours),
                AverageOvertimeHours = g.Average(x => (decimal)(x.EndTime - x.StartTime).TotalHours),
                ApprovedCount = g.Count(x => x.Status == OvertimeStatusCode.Approved),
                RejectedCount = g.Count(x => x.Status == OvertimeStatusCode.Rejected)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (totals is null)
            return new OvertimeAnalyticsResponse(0, 0, 0, 0);

        return new OvertimeAnalyticsResponse(
            Math.Round(totals.TotalOvertimeHours, 2),
            Math.Round(totals.AverageOvertimeHours, 2),
            totals.ApprovedCount,
            totals.RejectedCount
        );
    }
}
