using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.CalculatePayrollRun;
using Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.CreatePayrollPeriod;
using Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.LockPayrollPeriod;
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
}
