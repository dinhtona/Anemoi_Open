using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetAttendanceAnalytics;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetOvertimeAnalytics;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetPayrollAnalytics;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetWorkforceOverview;
using Anemoi.Hr.Application.Responses;
using MediatR;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetAnalyticsDashboard;

public sealed class GetAnalyticsDashboardHandler(ISender sender)
    : IQueryHandler<GetAnalyticsDashboardQuery, AnalyticsDashboardResponse>
{
    public async Task<AnalyticsDashboardResponse> Handle(
        GetAnalyticsDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var workforce = await sender.Send(new GetWorkforceOverviewQuery(), cancellationToken);
        var payroll = await sender.Send(new GetPayrollAnalyticsQuery(), cancellationToken);
        var attendance = await sender.Send(new GetAttendanceAnalyticsQuery(), cancellationToken);
        var overtime = await sender.Send(new GetOvertimeAnalyticsQuery(), cancellationToken);

        return new AnalyticsDashboardResponse(workforce, payroll, attendance, overtime);
    }
}
