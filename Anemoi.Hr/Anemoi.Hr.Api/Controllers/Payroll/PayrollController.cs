using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.CalculatePayrollRun;
using Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.CreatePayrollPeriod;
using Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.LockPayrollPeriod;
using Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.RecalculatePayrollRun;
using Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.SubmitPayrollRunForApproval;
using Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.ApprovePayrollRun;
using Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.RejectPayrollRun;
using Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.FinalizePayrollRun;
using Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.CancelPayrollRun;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollQueries.GetPayrollPeriods;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollQueries.GetPayrollRunDetail;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollQueries.GetPayrollRuns;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Api.Controllers.Payroll;

[ApiController]
[Route("api/hr/payroll/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class PayrollController(ISender sender) : ControllerBase
{
    [HttpPost]
    [HasPermission(HrPermissions.PayrollLock)]
    [ProducesResponseType(typeof(PayrollPeriodResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreatePayrollPeriod(
        [FromBody] CreatePayrollPeriodCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CreatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.PayrollCalculate)]
    [ProducesResponseType(typeof(PayrollRunDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CalculatePayrollRun(
        [FromBody] CalculatePayrollRunCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CalculatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.PayrollLock)]
    [ProducesResponseType(typeof(PayrollPeriodResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> LockPayrollPeriod(
        [FromBody] LockPayrollPeriodCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { UpdatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.PayrollView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<PayrollPeriodResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayrollPeriods(CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetPayrollPeriodsQuery(), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.PayrollView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<PayrollRunResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayrollRuns(
        [FromQuery] PayrollPeriodId payrollPeriodId,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetPayrollRunsQuery(payrollPeriodId), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.PayrollView)]
    [ProducesResponseType(typeof(PayrollRunDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayrollRunDetail(
        [FromRoute] PayrollRunId id,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetPayrollRunDetailQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.PayrollCalculate)]
    [ProducesResponseType(typeof(PayrollRunDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RecalculatePayrollRun(
        [FromBody] RecalculatePayrollRunCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CalculatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.PayrollCalculate)]
    [ProducesResponseType(typeof(PayrollRunDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SubmitPayrollRunForApproval(
        [FromBody] SubmitPayrollRunForApprovalCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { SubmittedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.PayrollApprove)]
    [ProducesResponseType(typeof(PayrollRunDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ApprovePayrollRun(
        [FromBody] ApprovePayrollRunCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { ApprovedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.PayrollApprove)]
    [ProducesResponseType(typeof(PayrollRunDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RejectPayrollRun(
        [FromBody] RejectPayrollRunCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { RejectedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.PayrollApprove)]
    [ProducesResponseType(typeof(PayrollRunDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> FinalizePayrollRun(
        [FromBody] FinalizePayrollRunCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { FinalizedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.PayrollCalculate)]
    [ProducesResponseType(typeof(PayrollRunDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelPayrollRun(
        [FromBody] CancelPayrollRunCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CancelledBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}
