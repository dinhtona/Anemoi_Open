using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.LeaveTypeCommands.ActivateLeaveType;
using Anemoi.Hr.Application.Cqrs.Commands.LeaveTypeCommands.CreateLeaveType;
using Anemoi.Hr.Application.Cqrs.Commands.LeaveTypeCommands.DeactivateLeaveType;
using Anemoi.Hr.Application.Cqrs.Commands.LeaveTypeCommands.UpdateLeaveType;
using Anemoi.Hr.Application.Cqrs.Queries.LeaveTypeQueries.GetLeaveTypeById;
using Anemoi.Hr.Application.Cqrs.Queries.LeaveTypeQueries.GetLeaveTypes;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.Leave;

[ApiController]
[Route("api/hr/leave-types")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class LeaveTypeController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(HrPermissions.LeaveTypeView)]
    [ProducesResponseType(typeof(PaginationResponse<LeaveTypeResponse>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<LeaveTypeResponse>> GetLeaveTypes(
        [FromQuery] GetLeaveTypesQuery query, CancellationToken cancellationToken)
    {
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.LeaveTypeView)]
    [ProducesResponseType(typeof(LeaveTypeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaveTypeById(
        [FromRoute] LeaveTypeId id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetLeaveTypeByIdQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.LeaveTypeManage)]
    [ProducesResponseType(typeof(LeaveTypeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateLeaveType(
        [FromBody] CreateLeaveTypeCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPut("{id}")]
    [HasPermission(HrPermissions.LeaveTypeManage)]
    [ProducesResponseType(typeof(LeaveTypeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateLeaveType(
        [FromRoute] LeaveTypeId id,
        [FromBody] UpdateLeaveTypeCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { Id = id }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}/activate")]
    [HasPermission(HrPermissions.LeaveTypeManage)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ActivateLeaveType(
        [FromRoute] LeaveTypeId id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new ActivateLeaveTypeCommand(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}/deactivate")]
    [HasPermission(HrPermissions.LeaveTypeManage)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivateLeaveType(
        [FromRoute] LeaveTypeId id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new DeactivateLeaveTypeCommand(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}
