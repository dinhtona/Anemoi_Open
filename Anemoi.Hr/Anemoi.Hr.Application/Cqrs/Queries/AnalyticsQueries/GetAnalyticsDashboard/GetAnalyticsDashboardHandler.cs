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
        var workforceTask = sender.Send(new GetWorkforceOverviewQuery(), cancellationToken);
        var payrollTask = sender.Send(new GetPayrollAnalyticsQuery(), cancellationToken);
        var attendanceTask = sender.Send(new GetAttendanceAnalyticsQuery(), cancellationToken);
        var overtimeTask = sender.Send(new GetOvertimeAnalyticsQuery(), cancellationToken);

        await Task.WhenAll(workforceTask, payrollTask, attendanceTask, overtimeTask);

        return new AnalyticsDashboardResponse(
            workforceTask.Result,
            payrollTask.Result,
            attendanceTask.Result,
            overtimeTask.Result
        );
    }
}
