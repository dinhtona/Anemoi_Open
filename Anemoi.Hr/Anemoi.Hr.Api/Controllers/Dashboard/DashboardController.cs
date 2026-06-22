using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Queries.LifecycleQueries.GetLifecycleSummary;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.Dashboard;

[ApiController]
[Route("api/hr/dashboard")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class DashboardController(ISender sender) : ControllerBase
{
    [HttpGet("lifecycle-summary")]
    [HasPermission(HrPermissions.DashboardView)]
    [ProducesResponseType(typeof(DashboardLifecycleSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLifecycleSummary(CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetLifecycleSummaryQuery(), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}
