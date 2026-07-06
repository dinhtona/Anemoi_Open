using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.ActivateInsuranceRuleSet;
using Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.CalculateInsurance;
using Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.CreateInsuranceContributionRule;
using Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.CreateInsuranceRuleSet;
using Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.DeactivateInsuranceRuleSet;
using Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.DeleteInsuranceContributionRule;
using Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.UpdateInsuranceContributionRule;
using Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.UpdateInsuranceRuleSet;
using Anemoi.Hr.Application.Cqrs.Queries.InsuranceQueries.GetInsuranceCalculationSnapshotDetail;
using Anemoi.Hr.Application.Cqrs.Queries.InsuranceQueries.GetInsuranceCalculationSnapshots;
using Anemoi.Hr.Application.Cqrs.Queries.InsuranceQueries.GetInsuranceContributionReport;
using Anemoi.Hr.Application.Cqrs.Queries.InsuranceQueries.GetInsuranceRuleSetDetail;
using Anemoi.Hr.Application.Cqrs.Queries.InsuranceQueries.GetInsuranceRuleSets;
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

namespace Anemoi.Hr.Api.Controllers.Insurance;

[ApiController]
[Route("api/hr/insurance")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class InsuranceManagementController(ISender sender) : ControllerBase
{
    [HttpGet("rule-sets")]
    [HasPermission(HrPermissions.InsuranceView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<InsuranceRuleSetResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInsuranceRuleSets(
        [FromQuery] string? countryCode,
        [FromQuery] string? insuranceType,
        CancellationToken cancellationToken)
    {
        var query = new GetInsuranceRuleSetsQuery(countryCode, insuranceType);
        var res = await sender.Send(query, cancellationToken);
        return Ok(res);
    }

    [HttpGet("rule-sets/{id}")]
    [HasPermission(HrPermissions.InsuranceView)]
    [ProducesResponseType(typeof(InsuranceRuleSetResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInsuranceRuleSetDetail(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var query = new GetInsuranceRuleSetDetailQuery(id);
        var res = await sender.Send(query, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("rule-sets")]
    [HasPermission(HrPermissions.InsuranceManage)]
    [ProducesResponseType(typeof(CreateInsuranceRuleSetResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateInsuranceRuleSet(
        [FromBody] CreateInsuranceRuleSetCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CreatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPut("rule-sets/{id}")]
    [HasPermission(HrPermissions.InsuranceManage)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateInsuranceRuleSet(
        [FromRoute] string id,
        [FromBody] UpdateInsuranceRuleSetCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { Id = id, UpdatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("rule-sets/{id}/activate")]
    [HasPermission(HrPermissions.InsuranceManage)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ActivateInsuranceRuleSet(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var command = new ActivateInsuranceRuleSetCommand(id, HttpContext.GetUserId());
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("rule-sets/{id}/deactivate")]
    [HasPermission(HrPermissions.InsuranceManage)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivateInsuranceRuleSet(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateInsuranceRuleSetCommand(id, HttpContext.GetUserId());
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("rule-sets/{id}/contribution-rules")]
    [HasPermission(HrPermissions.InsuranceManage)]
    [ProducesResponseType(typeof(CreateInsuranceContributionRuleResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateInsuranceContributionRule(
        [FromRoute] string id,
        [FromBody] CreateInsuranceContributionRuleCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { RuleSetId = id }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPut("contribution-rules/{id}")]
    [HasPermission(HrPermissions.InsuranceManage)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateInsuranceContributionRule(
        [FromRoute] string id,
        [FromBody] UpdateInsuranceContributionRuleCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { Id = id }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpDelete("contribution-rules/{id}")]
    [HasPermission(HrPermissions.InsuranceManage)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteInsuranceContributionRule(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteInsuranceContributionRuleCommand(id);
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("calculate")]
    [HasPermission(HrPermissions.InsuranceCalculate)]
    [ProducesResponseType(typeof(CalculateInsuranceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CalculateInsurance(
        [FromBody] CalculateInsuranceCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CalculatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("calculation-snapshots")]
    [HasPermission(HrPermissions.InsuranceView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<InsuranceCalculationSnapshotResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInsuranceCalculationSnapshots(
        [FromQuery] string? employeeId,
        [FromQuery] string? insuranceType,
        [FromQuery] string? sourceModule,
        [FromQuery] string? sourceReferenceId,
        CancellationToken cancellationToken)
    {
        var query = new GetInsuranceCalculationSnapshotsQuery(employeeId, insuranceType, sourceModule, sourceReferenceId);
        var res = await sender.Send(query, cancellationToken);
        return Ok(res);
    }

    [HttpGet("calculation-snapshots/{id}")]
    [HasPermission(HrPermissions.InsuranceView)]
    [ProducesResponseType(typeof(InsuranceCalculationSnapshotResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInsuranceCalculationSnapshotDetail(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var query = new GetInsuranceCalculationSnapshotDetailQuery(id);
        var res = await sender.Send(query, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("reports/contributions")]
    [HasPermission(HrPermissions.InsuranceReport)]
    [ProducesResponseType(typeof(IReadOnlyCollection<InsuranceCalculationSnapshotResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInsuranceContributionReport(
        [FromQuery] DateOnly periodStart,
        [FromQuery] DateOnly periodEnd,
        [FromQuery] string? countryCode,
        [FromQuery] string? insuranceType,
        CancellationToken cancellationToken)
    {
        var query = new GetInsuranceContributionReportQuery(periodStart, periodEnd, countryCode, insuranceType);
        var res = await sender.Send(query, cancellationToken);
        return Ok(res);
    }
}
