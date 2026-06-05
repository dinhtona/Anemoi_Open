using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Queries.DashboardQueries.GetDashboardOverview;
using Anemoi.Hr.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Api.Controllers.Dashboard;

[ApiController]
[Route("api/hr/dashboard/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class DashboardController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(HrPermissions.DashboardView)]
    [ProducesResponseType(typeof(DashboardOverviewResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardOverview(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetDashboardOverviewQuery(), cancellationToken);
        return Ok(result);
    }
}
