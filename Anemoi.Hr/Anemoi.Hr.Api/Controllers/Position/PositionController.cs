using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Queries.PositionQueries.GetPositions;
using Anemoi.Hr.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.Position;

[ApiController]
[Route("api/hr/position/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class PositionController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(HrPermissions.PositionView)]
    [ProducesResponseType(typeof(PaginationResponse<PositionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPositions([FromQuery] GetPositionsQuery query, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(query, cancellationToken));
    }
}
