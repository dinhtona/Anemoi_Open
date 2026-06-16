using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetApplicationsByStage;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetCandidateSourceEffectiveness;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetHiringByDepartment;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentAnalyticsDashboard;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentOverview;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetTimeToHire;
using Anemoi.Hr.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Api.Controllers.Recruitment;

[ApiController]
[Route("api/hr/recruitment/analytics")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class RecruitmentAnalyticsController(ISender sender) : ControllerBase
{
    [HttpGet("overview")]
    [HasPermission(HrPermissions.RecruitmentAnalytics)]
    [ProducesResponseType(typeof(RecruitmentOverviewResponse), StatusCodes.Status200OK)]
    public async Task<RecruitmentOverviewResponse> GetRecruitmentOverview(
        [FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        return await sender.Send(new GetRecruitmentOverviewQuery(fromDate, toDate), cancellationToken);
    }

    [HttpGet("applications-by-stage")]
    [HasPermission(HrPermissions.RecruitmentAnalytics)]
    [ProducesResponseType(typeof(IReadOnlyCollection<ApplicationsByStageItem>), StatusCodes.Status200OK)]
    public async Task<IReadOnlyCollection<ApplicationsByStageItem>> GetApplicationsByStage(
        [FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        return await sender.Send(new GetApplicationsByStageQuery(fromDate, toDate), cancellationToken);
    }

    [HttpGet("hiring-by-department")]
    [HasPermission(HrPermissions.RecruitmentAnalytics)]
    [ProducesResponseType(typeof(IReadOnlyCollection<HiringByDepartmentItem>), StatusCodes.Status200OK)]
    public async Task<IReadOnlyCollection<HiringByDepartmentItem>> GetHiringByDepartment(
        [FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        return await sender.Send(new GetHiringByDepartmentQuery(fromDate, toDate), cancellationToken);
    }

    [HttpGet("source-effectiveness")]
    [HasPermission(HrPermissions.RecruitmentAnalytics)]
    [ProducesResponseType(typeof(IReadOnlyCollection<CandidateSourceEffectivenessItem>), StatusCodes.Status200OK)]
    public async Task<IReadOnlyCollection<CandidateSourceEffectivenessItem>> GetCandidateSourceEffectiveness(
        [FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        return await sender.Send(new GetCandidateSourceEffectivenessQuery(fromDate, toDate), cancellationToken);
    }

    [HttpGet("time-to-hire")]
    [HasPermission(HrPermissions.RecruitmentAnalytics)]
    [ProducesResponseType(typeof(TimeToHireResponse), StatusCodes.Status200OK)]
    public async Task<TimeToHireResponse> GetTimeToHire(
        [FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        return await sender.Send(new GetTimeToHireQuery(fromDate, toDate), cancellationToken);
    }

    [HttpGet("dashboard")]
    [HasPermission(HrPermissions.RecruitmentAnalytics)]
    [ProducesResponseType(typeof(RecruitmentAnalyticsDashboardResponse), StatusCodes.Status200OK)]
    public async Task<RecruitmentAnalyticsDashboardResponse> GetRecruitmentAnalyticsDashboard(
        [FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        return await sender.Send(new GetRecruitmentAnalyticsDashboardQuery(fromDate, toDate), cancellationToken);
    }
}
