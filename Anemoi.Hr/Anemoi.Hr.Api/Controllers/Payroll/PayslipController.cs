using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.GeneratePayslipsForPayrollRun;
using Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.PublishPayslip;
using Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.CancelPayslip;
using Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipDetail;
using Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipsByPayrollRun;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Api.Controllers.Payroll;

[ApiController]
[Route("api/hr/payroll/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class PayslipController(ISender sender) : ControllerBase
{
    [HttpPost]
    [HasPermission(HrPermissions.PayrollCalculate)]
    [ProducesResponseType(typeof(IReadOnlyCollection<PayslipResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GeneratePayslipsForPayrollRun(
        [FromBody] GeneratePayslipsForPayrollRunCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { GeneratedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.PayrollApprove)]
    [ProducesResponseType(typeof(PayslipResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> PublishPayslip(
        [FromBody] PublishPayslipCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { PublishedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.PayrollCalculate)]
    [ProducesResponseType(typeof(PayslipResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelPayslip(
        [FromBody] CancelPayslipCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CancelledBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.PayrollView)]
    [ProducesResponseType(typeof(PayslipResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayslipDetail(
        [FromRoute] PayslipId id,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetPayslipDetailQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.PayrollView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<PayslipResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayslipsByPayrollRun(
        [FromQuery] PayrollRunId payrollRunId,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetPayslipsByPayrollRunQuery(payrollRunId), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}
