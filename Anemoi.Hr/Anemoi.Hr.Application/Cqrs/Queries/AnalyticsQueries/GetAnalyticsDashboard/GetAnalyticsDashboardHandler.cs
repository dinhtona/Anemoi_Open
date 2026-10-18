using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetAttendanceAnalytics;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetOvertimeAnalytics;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetPayrollAnalytics;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetWorkforceOverview;
using Anemoi.Hr.Application.Responses;
using MediatR;
using OneOf;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetAnalyticsDashboard;

public sealed class GetAnalyticsDashboardHandler(ISender sender)
    : IQueryHandler<GetAnalyticsDashboardQuery, OneOf<AnalyticsDashboardResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<AnalyticsDashboardResponse, ErrorDetailResponse>> Handle(
        GetAnalyticsDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var workforce = await sender.Send(new GetWorkforceOverviewQuery(), cancellationToken);
        var payroll = await sender.Send(new GetPayrollAnalyticsQuery(), cancellationToken);
        var attendance = await sender.Send(new GetAttendanceAnalyticsQuery(), cancellationToken);
        var overtime = await sender.Send(new GetOvertimeAnalyticsQuery(), cancellationToken);

        if (workforce.IsT1) return workforce.AsT1;
        if (payroll.IsT1) return payroll.AsT1;
        if (attendance.IsT1) return attendance.AsT1;
        if (overtime.IsT1) return overtime.AsT1;

        return new AnalyticsDashboardResponse(workforce.AsT0, payroll.AsT0, attendance.AsT0, overtime.AsT0);
    }
}
