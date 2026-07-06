using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.OvertimeRuleCommands.ActivateOvertimeRule;
using Anemoi.Hr.Application.Cqrs.Commands.OvertimeRuleCommands.CreateOvertimeRule;
using Anemoi.Hr.Application.Cqrs.Commands.OvertimeRuleCommands.DeactivateOvertimeRule;
using Anemoi.Hr.Application.Cqrs.Commands.OvertimeRuleCommands.UpdateOvertimeRule;
using Anemoi.Hr.Application.Cqrs.Queries.OvertimeRuleQueries.GetOvertimeRuleById;
using Anemoi.Hr.Application.Cqrs.Queries.OvertimeRuleQueries.GetOvertimeRules;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.Payroll;

[ApiController]
[Route("api/hr/overtime-rules")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class OvertimeRuleController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(HrPermissions.OvertimeRuleView)]
    [ProducesResponseType(typeof(PaginationResponse<OvertimeRuleResponse>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<OvertimeRuleResponse>> GetOvertimeRules(
        [FromQuery] GetOvertimeRulesQuery query, CancellationToken cancellationToken)
    {
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.OvertimeRuleView)]
    [ProducesResponseType(typeof(OvertimeRuleResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOvertimeRuleById(
        [FromRoute] OvertimeRuleId id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetOvertimeRuleByIdQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.OvertimeRuleManage)]
    [ProducesResponseType(typeof(OvertimeRuleResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateOvertimeRule(
        [FromBody] CreateOvertimeRuleCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPut("{id}")]
    [HasPermission(HrPermissions.OvertimeRuleManage)]
    [ProducesResponseType(typeof(OvertimeRuleResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateOvertimeRule(
        [FromRoute] OvertimeRuleId id,
        [FromBody] UpdateOvertimeRuleCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { Id = id }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}/activate")]
    [HasPermission(HrPermissions.OvertimeRuleManage)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ActivateOvertimeRule(
        [FromRoute] OvertimeRuleId id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new ActivateOvertimeRuleCommand(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}/deactivate")]
    [HasPermission(HrPermissions.OvertimeRuleManage)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivateOvertimeRule(
        [FromRoute] OvertimeRuleId id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new DeactivateOvertimeRuleCommand(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}
