using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.OvertimeRequestCommands.CreateOvertimeRequest;
using Anemoi.Hr.Application.Cqrs.Commands.OvertimeRequestCommands.ApproveOvertimeRequest;
using Anemoi.Hr.Application.Cqrs.Commands.OvertimeRequestCommands.RejectOvertimeRequest;
using Anemoi.Hr.Application.Cqrs.Commands.OvertimeRequestCommands.CancelOvertimeRequest;
using Anemoi.Hr.Application.Cqrs.Queries.OvertimeRequestQueries.GetOvertimeRequestById;
using Anemoi.Hr.Application.Cqrs.Queries.OvertimeRequestQueries.GetOvertimeRequests;
using Anemoi.Hr.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.Payroll;

[ApiController]
[Route("api/hr/overtime/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class OvertimeController(ISender sender) : ControllerBase
{
    [HttpPost]
    [HasPermission(HrPermissions.OvertimeCreate)]
    [ProducesResponseType(typeof(OvertimeRequestIdResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateOvertimeRequestCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}")]
    [HasPermission(HrPermissions.OvertimeApprove)]
    public async Task<IActionResult> Approve([FromRoute] OvertimeRequestId id,
        [FromBody] ApproveOvertimeRequestCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command with { Id = id }, cancellationToken);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPost("{id}")]
    [HasPermission(HrPermissions.OvertimeApprove)]
    public async Task<IActionResult> Reject([FromRoute] OvertimeRequestId id,
        [FromBody] RejectOvertimeRequestCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command with { Id = id }, cancellationToken);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPost("{id}")]
    [HasPermission(HrPermissions.OvertimeCreate)]
    public async Task<IActionResult> Cancel([FromRoute] OvertimeRequestId id,
        [FromBody] CancelOvertimeRequestCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command with { Id = id }, cancellationToken);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.OvertimeView)]
    [ProducesResponseType(typeof(OvertimeRequestResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById([FromRoute] OvertimeRequestId id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOvertimeRequestByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    [HasPermission(HrPermissions.OvertimeView)]
    [ProducesResponseType(typeof(PaginationResponse<OvertimeRequestResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged([FromQuery] GetOvertimeRequestsQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(query, cancellationToken));
    }
}
