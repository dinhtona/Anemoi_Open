using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.ActivateTaxRuleSet;
using Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.CalculateTax;
using Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.CreateTaxBracket;
using Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.CreateTaxDeductionRule;
using Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.CreateTaxRuleSet;
using Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.DeactivateTaxRuleSet;
using Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.DeleteTaxBracket;
using Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.UpdateTaxBracket;
using Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.UpdateTaxDeductionRule;
using Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.UpdateTaxRuleSet;
using Anemoi.Hr.Application.Cqrs.Queries.TaxationQueries.GetTaxCalculationSnapshotDetail;
using Anemoi.Hr.Application.Cqrs.Queries.TaxationQueries.GetTaxCalculationSnapshots;
using Anemoi.Hr.Application.Cqrs.Queries.TaxationQueries.GetTaxRuleSetDetail;
using Anemoi.Hr.Application.Cqrs.Queries.TaxationQueries.GetTaxRuleSets;
using Anemoi.Hr.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Api.Controllers.Taxation;

[ApiController]
[Route("api/hr/tax")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class TaxManagementController(ISender sender) : ControllerBase
{
    [HttpGet("rule-sets")]
    [HasPermission(HrPermissions.TaxView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<TaxRuleSetResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTaxRuleSets(
        [FromQuery] string countryCode,
        [FromQuery] string taxType,
        CancellationToken cancellationToken)
    {
        var query = new GetTaxRuleSetsQuery(countryCode, taxType);
        var res = await sender.Send(query, cancellationToken);
        return Ok(res);
    }

    [HttpGet("rule-sets/{id}")]
    [HasPermission(HrPermissions.TaxView)]
    [ProducesResponseType(typeof(TaxRuleSetResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTaxRuleSetDetail(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var query = new GetTaxRuleSetDetailQuery(id);
        var res = await sender.Send(query, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("rule-sets")]
    [HasPermission(HrPermissions.TaxManage)]
    [ProducesResponseType(typeof(CreateTaxRuleSetResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateTaxRuleSet(
        [FromBody] CreateTaxRuleSetCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CreatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPut("rule-sets/{id}")]
    [HasPermission(HrPermissions.TaxManage)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateTaxRuleSet(
        [FromRoute] string id,
        [FromBody] UpdateTaxRuleSetCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { Id = id, UpdatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("rule-sets/{id}/activate")]
    [HasPermission(HrPermissions.TaxManage)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ActivateTaxRuleSet(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var command = new ActivateTaxRuleSetCommand(id, HttpContext.GetUserId());
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("rule-sets/{id}/deactivate")]
    [HasPermission(HrPermissions.TaxManage)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivateTaxRuleSet(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateTaxRuleSetCommand(id, HttpContext.GetUserId());
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("rule-sets/{id}/brackets")]
    [HasPermission(HrPermissions.TaxManage)]
    [ProducesResponseType(typeof(CreateTaxBracketResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateTaxBracket(
        [FromRoute] string id,
        [FromBody] CreateTaxBracketCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { TaxRuleSetId = id }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPut("brackets/{id}")]
    [HasPermission(HrPermissions.TaxManage)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateTaxBracket(
        [FromRoute] string id,
        [FromBody] UpdateTaxBracketCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { Id = id }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpDelete("brackets/{id}")]
    [HasPermission(HrPermissions.TaxManage)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteTaxBracket(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteTaxBracketCommand(id);
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("rule-sets/{id}/deduction-rules")]
    [HasPermission(HrPermissions.TaxManage)]
    [ProducesResponseType(typeof(CreateTaxDeductionRuleResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateTaxDeductionRule(
        [FromRoute] string id,
        [FromBody] CreateTaxDeductionRuleCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { TaxRuleSetId = id }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPut("deduction-rules/{id}")]
    [HasPermission(HrPermissions.TaxManage)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateTaxDeductionRule(
        [FromRoute] string id,
        [FromBody] UpdateTaxDeductionRuleCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { Id = id }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("calculate")]
    [HasPermission(HrPermissions.TaxCalculate)]
    [ProducesResponseType(typeof(CalculateTaxResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CalculateTax(
        [FromBody] CalculateTaxCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CalculatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("calculation-snapshots")]
    [HasPermission(HrPermissions.TaxView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<TaxCalculationSnapshotResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTaxCalculationSnapshots(
        [FromQuery] string employeeId,
        [FromQuery] string sourceModule,
        [FromQuery] Guid? sourceReferenceId,
        CancellationToken cancellationToken)
    {
        var query = new GetTaxCalculationSnapshotsQuery(employeeId, sourceModule, sourceReferenceId);
        var res = await sender.Send(query, cancellationToken);
        return Ok(res);
    }

    [HttpGet("calculation-snapshots/{id}")]
    [HasPermission(HrPermissions.TaxView)]
    [ProducesResponseType(typeof(TaxCalculationSnapshotResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTaxCalculationSnapshotDetail(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var query = new GetTaxCalculationSnapshotDetailQuery(id);
        var res = await sender.Send(query, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}
