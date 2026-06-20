using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.LeavePolicyCommands.ActivateLeavePolicy;
using Anemoi.Hr.Application.Cqrs.Commands.LeavePolicyCommands.CreateLeavePolicy;
using Anemoi.Hr.Application.Cqrs.Commands.LeavePolicyCommands.DeactivateLeavePolicy;
using Anemoi.Hr.Application.Cqrs.Commands.LeavePolicyCommands.UpdateLeavePolicy;
using Anemoi.Hr.Application.Cqrs.Queries.LeavePolicyQueries.GetLeavePolicies;
using Anemoi.Hr.Application.Cqrs.Queries.LeavePolicyQueries.GetLeavePolicy;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.Leave;

[ApiController]
[Route("api/hr/leave-policies")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class LeavePolicySettingsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(HrPermissions.LeavePolicySettingsView)]
    [ProducesResponseType(typeof(PaginationResponse<LeavePolicyResponse>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<LeavePolicyResponse>> GetLeavePolicies(
        [FromQuery] GetLeavePoliciesQuery query, CancellationToken cancellationToken)
    {
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.LeavePolicySettingsView)]
    [ProducesResponseType(typeof(LeavePolicyResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeavePolicy(
        [FromRoute] LeavePolicyId id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetLeavePolicyQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.LeavePolicySettingsManage)]
    [ProducesResponseType(typeof(LeavePolicyIdResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateLeavePolicy(
        [FromBody] CreateLeavePolicyCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPut("{id}")]
    [HasPermission(HrPermissions.LeavePolicySettingsManage)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateLeavePolicy(
        [FromRoute] LeavePolicyId id,
        [FromBody] UpdateLeavePolicyCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { Id = id }, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPost("{id}/activate")]
    [HasPermission(HrPermissions.LeavePolicySettingsManage)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ActivateLeavePolicy(
        [FromRoute] LeavePolicyId id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new ActivateLeavePolicyCommand(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}/deactivate")]
    [HasPermission(HrPermissions.LeavePolicySettingsManage)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivateLeavePolicy(
        [FromRoute] LeavePolicyId id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new DeactivateLeavePolicyCommand(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}
