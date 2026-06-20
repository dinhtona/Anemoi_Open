using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.PositionCommands.ActivatePosition;
using Anemoi.Hr.Application.Cqrs.Commands.PositionCommands.CreatePosition;
using Anemoi.Hr.Application.Cqrs.Commands.PositionCommands.DeactivatePosition;
using Anemoi.Hr.Application.Cqrs.Commands.PositionCommands.UpdatePosition;
using Anemoi.Hr.Application.Cqrs.Queries.PositionQueries.GetPositionById;
using Anemoi.Hr.Application.Cqrs.Queries.PositionQueries.GetPositions;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.Position;

[ApiController]
[Route("api/hr/positions")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class PositionController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(HrPermissions.PositionView)]
    [ProducesResponseType(typeof(PaginationResponse<PositionResponse>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<PositionResponse>> GetPositions(
        [FromQuery] GetPositionsQuery query, CancellationToken cancellationToken)
    {
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.PositionView)]
    [ProducesResponseType(typeof(PositionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPositionById(
        [FromRoute] PositionId id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetPositionByIdQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.PositionManage)]
    [ProducesResponseType(typeof(PositionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreatePosition(
        [FromBody] CreatePositionCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPut("{id}")]
    [HasPermission(HrPermissions.PositionManage)]
    [ProducesResponseType(typeof(PositionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePosition(
        [FromRoute] PositionId id,
        [FromBody] UpdatePositionCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { Id = id }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}/activate")]
    [HasPermission(HrPermissions.PositionManage)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ActivatePosition(
        [FromRoute] PositionId id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new ActivatePositionCommand(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}/deactivate")]
    [HasPermission(HrPermissions.PositionManage)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivatePosition(
        [FromRoute] PositionId id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new DeactivatePositionCommand(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}
