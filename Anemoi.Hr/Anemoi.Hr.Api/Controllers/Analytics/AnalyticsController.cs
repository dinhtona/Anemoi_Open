using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetAnalyticsDashboard;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetAttendanceAnalytics;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetDepartmentCostAnalytics;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetHeadcountTrend;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetOvertimeAnalytics;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetPayrollAnalytics;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetTopEarners;
using Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetWorkforceOverview;
using Anemoi.Hr.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Api.Controllers.Analytics;

[ApiController]
[Route("api/hr/analytics")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class AnalyticsController(ISender sender) : ControllerBase
{
    [HttpGet("dashboard")]
    [HasPermission(HrPermissions.AnalyticsView)]
    [ProducesResponseType(typeof(AnalyticsDashboardResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAnalyticsDashboardQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("workforce")]
    [HasPermission(HrPermissions.AnalyticsView)]
    [ProducesResponseType(typeof(WorkforceOverviewResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkforce(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetWorkforceOverviewQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("headcount-trend")]
    [HasPermission(HrPermissions.AnalyticsView)]
    [ProducesResponseType(typeof(ICollection<HeadcountTrendItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHeadcountTrend(
        [FromQuery] GetHeadcountTrendQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("payroll")]
    [HasPermission(HrPermissions.AnalyticsView)]
    [ProducesResponseType(typeof(PayrollAnalyticsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayroll(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPayrollAnalyticsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("overtime")]
    [HasPermission(HrPermissions.AnalyticsView)]
    [ProducesResponseType(typeof(OvertimeAnalyticsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOvertime(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOvertimeAnalyticsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("attendance")]
    [HasPermission(HrPermissions.AnalyticsView)]
    [ProducesResponseType(typeof(AttendanceAnalyticsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAttendance(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAttendanceAnalyticsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("department-cost")]
    [HasPermission(HrPermissions.AnalyticsView)]
    [ProducesResponseType(typeof(ICollection<DepartmentCostItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartmentCost(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetDepartmentCostAnalyticsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("top-earners")]
    [HasPermission(HrPermissions.AnalyticsView)]
    [ProducesResponseType(typeof(ICollection<TopEarnerItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopEarners(
        [FromQuery] GetTopEarnersQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return Ok(result);
    }
}
