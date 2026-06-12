using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.ExportPayrollItemDetailCsv;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.ExportPayrollRunSummaryCsv;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.ExportPayslipSummaryCsv;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.GetPayrollItemDetailReport;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.GetPayrollRunSummaryReport;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.GetPayslipSummaryReport;
using Anemoi.Hr.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.Payroll;

[ApiController]
[Route("api/hr/payroll/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class PayrollReportingController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(HrPermissions.PayrollView)]
    [ProducesResponseType(typeof(PaginationResponse<PayrollRunSummaryReportItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayrollRunSummary(
        [FromQuery] GetPayrollRunSummaryReportQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [HasPermission(HrPermissions.PayrollView)]
    [ProducesResponseType(typeof(PaginationResponse<PayrollItemDetailReportItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayrollItemDetail(
        [FromQuery] GetPayrollItemDetailReportQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [HasPermission(HrPermissions.PayrollView)]
    [ProducesResponseType(typeof(PaginationResponse<PayslipSummaryReportItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayslipSummary(
        [FromQuery] GetPayslipSummaryReportQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [HasPermission(HrPermissions.PayrollExport)]
    [Produces("text/csv")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportPayrollRunSummaryCsv(
        [FromQuery] ExportPayrollRunSummaryCsvQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return File(result.Content, result.ContentType, result.FileName);
    }

    [HttpGet]
    [HasPermission(HrPermissions.PayrollExport)]
    [Produces("text/csv")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportPayrollItemDetailCsv(
        [FromQuery] ExportPayrollItemDetailCsvQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return File(result.Content, result.ContentType, result.FileName);
    }

    [HttpGet]
    [HasPermission(HrPermissions.PayrollExport)]
    [Produces("text/csv")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportPayslipSummaryCsv(
        [FromQuery] ExportPayslipSummaryCsvQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return File(result.Content, result.ContentType, result.FileName);
    }
}
