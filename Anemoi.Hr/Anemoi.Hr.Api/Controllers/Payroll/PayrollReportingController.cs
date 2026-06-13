using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.ExportPayrollItemDetailCsv;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.ExportPayrollRunSummaryCsv;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.ExportPayslipSummaryCsv;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.GetPayrollItemDetailReport;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.GetPayrollRunSummaryReport;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.GetPayslipSummaryReport;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.GetPayrollCostSummaryReport;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.GetPayrollCostDepartmentReport;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.GetPayrollCostEmployeeReport;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.GetPayrollVarianceReport;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.GetPayslipDeliveryReport;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.ExportPayrollCostSummaryCsv;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.ExportPayrollCostDepartmentCsv;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.ExportPayrollCostEmployeeCsv;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.ExportPayrollVarianceCsv;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.ExportPayslipDeliveryCsv;
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
    [HasPermission(HrPermissions.PayrollReportingView)]
    [ProducesResponseType(typeof(PaginationResponse<PayrollRunSummaryReportItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayrollRunSummary(
        [FromQuery] GetPayrollRunSummaryReportQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [HasPermission(HrPermissions.PayrollReportingView)]
    [ProducesResponseType(typeof(PaginationResponse<PayrollItemDetailReportItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayrollItemDetail(
        [FromQuery] GetPayrollItemDetailReportQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [HasPermission(HrPermissions.PayrollReportingView)]
    [ProducesResponseType(typeof(PaginationResponse<PayslipSummaryReportItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayslipSummary(
        [FromQuery] GetPayslipSummaryReportQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [HasPermission(HrPermissions.PayrollReportingExport)]
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
    [HasPermission(HrPermissions.PayrollReportingExport)]
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
    [HasPermission(HrPermissions.PayrollReportingExport)]
    [Produces("text/csv")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportPayslipSummaryCsv(
        [FromQuery] ExportPayslipSummaryCsvQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return File(result.Content, result.ContentType, result.FileName);
    }

    // Phase 23 endpoints

    [HttpGet]
    [HasPermission(HrPermissions.PayrollReportingView)]
    [ProducesResponseType(typeof(PaginationResponse<PayrollCostSummaryReportItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayrollCostSummary(
        [FromQuery] GetPayrollCostSummaryReportQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [HasPermission(HrPermissions.PayrollReportingView)]
    [ProducesResponseType(typeof(PaginationResponse<PayrollCostDepartmentReportItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayrollCostDepartment(
        [FromQuery] GetPayrollCostDepartmentReportQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [HasPermission(HrPermissions.PayrollReportingView)]
    [ProducesResponseType(typeof(PaginationResponse<PayrollCostEmployeeReportItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayrollCostEmployee(
        [FromQuery] GetPayrollCostEmployeeReportQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [HasPermission(HrPermissions.PayrollReportingView)]
    [ProducesResponseType(typeof(PaginationResponse<PayrollVarianceReportItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayrollVariance(
        [FromQuery] GetPayrollVarianceReportQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [HasPermission(HrPermissions.PayrollReportingView)]
    [ProducesResponseType(typeof(PaginationResponse<PayslipDeliveryReportItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayslipDeliveryReport(
        [FromQuery] GetPayslipDeliveryReportQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [HasPermission(HrPermissions.PayrollReportingExport)]
    [Produces("text/csv")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportPayrollCostSummaryCsv(
        [FromQuery] ExportPayrollCostSummaryCsvQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return File(result.Content, result.ContentType, result.FileName);
    }

    [HttpGet]
    [HasPermission(HrPermissions.PayrollReportingExport)]
    [Produces("text/csv")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportPayrollCostDepartmentCsv(
        [FromQuery] ExportPayrollCostDepartmentCsvQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return File(result.Content, result.ContentType, result.FileName);
    }

    [HttpGet]
    [HasPermission(HrPermissions.PayrollReportingExport)]
    [Produces("text/csv")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportPayrollCostEmployeeCsv(
        [FromQuery] ExportPayrollCostEmployeeCsvQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return File(result.Content, result.ContentType, result.FileName);
    }

    [HttpGet]
    [HasPermission(HrPermissions.PayrollReportingExport)]
    [Produces("text/csv")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportPayrollVarianceCsv(
        [FromQuery] ExportPayrollVarianceCsvQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return File(result.Content, result.ContentType, result.FileName);
    }

    [HttpGet]
    [HasPermission(HrPermissions.PayrollReportingExport)]
    [Produces("text/csv")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportPayslipDeliveryCsv(
        [FromQuery] ExportPayslipDeliveryCsvQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return File(result.Content, result.ContentType, result.FileName);
    }
}
