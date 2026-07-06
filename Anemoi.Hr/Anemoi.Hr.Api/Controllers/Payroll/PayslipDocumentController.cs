using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.GeneratePayslipPdf;
using Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.GeneratePayslipPdfsForPayrollRun;
using Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.SendPayslipEmail;
using Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.SendPayslipEmailsForPayrollRun;
using Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipDocuments;
using Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipDocumentDownload;
using Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipEmailDeliveries;
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

namespace Anemoi.Hr.Api.Controllers.Payroll;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class PayslipDocumentController(ISender sender) : ControllerBase
{
    [HttpGet("/api/hr/payslips/{payslipId}/documents")]
    [HasPermission(HrPermissions.PayslipDocumentView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<PayslipDocumentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayslipDocuments(
        [FromRoute] Guid payslipId,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetPayslipDocumentsQuery(payslipId), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("/api/hr/payslips/{payslipId}/documents/generate")]
    [HasPermission(HrPermissions.PayslipDocumentGenerate)]
    [ProducesResponseType(typeof(PayslipDocumentResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GeneratePayslipPdf(
        [FromRoute] Guid payslipId,
        [FromBody] GeneratePayslipPdfBody body,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GeneratePayslipPdfCommand(
            payslipId,
            HttpContext.GetUserId(),
            body.ForceRegenerate), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("/api/hr/payslips/{payslipId}/documents/{documentId}/download")]
    [HasPermission(HrPermissions.PayslipDocumentView)]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> DownloadPayslipDocument(
        [FromRoute] Guid payslipId,
        [FromRoute] Guid documentId,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetPayslipDocumentDownloadQuery(payslipId, documentId), cancellationToken);
        return res.Match<IActionResult>(
            download => File(download.Stream, download.ContentType, download.FileName),
            BadRequest);
    }

    [HttpPost("/api/hr/payroll-runs/{payrollRunId}/payslip-documents/generate")]
    [HasPermission(HrPermissions.PayslipDocumentGenerate)]
    [ProducesResponseType(typeof(PayslipBatchResultResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GeneratePayslipPdfsForPayrollRun(
        [FromRoute] Guid payrollRunId,
        [FromBody] GeneratePayslipPdfsForPayrollRunBody body,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GeneratePayslipPdfsForPayrollRunCommand(
            payrollRunId,
            HttpContext.GetUserId(),
            body.ForceRegenerate), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("/api/hr/payslips/{payslipId}/email/send")]
    [HasPermission(HrPermissions.PayslipEmailSend)]
    [ProducesResponseType(typeof(PayslipEmailDeliveryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SendPayslipEmail(
        [FromRoute] Guid payslipId,
        [FromBody] SendPayslipEmailBody body,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new SendPayslipEmailCommand(
            payslipId,
            HttpContext.GetUserId(),
            body.ToEmail), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("/api/hr/payroll-runs/{payrollRunId}/payslip-emails/send")]
    [HasPermission(HrPermissions.PayslipEmailSend)]
    [ProducesResponseType(typeof(PayslipBatchResultResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SendPayslipEmailsForPayrollRun(
        [FromRoute] Guid payrollRunId,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new SendPayslipEmailsForPayrollRunCommand(
            payrollRunId,
            HttpContext.GetUserId()), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("/api/hr/payslips/{payslipId}/email-deliveries")]
    [HasPermission(HrPermissions.PayslipDocumentView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<PayslipEmailDeliveryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayslipEmailDeliveries(
        [FromRoute] Guid payslipId,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetPayslipEmailDeliveriesQuery(payslipId), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}

public sealed class GeneratePayslipPdfBody
{
    public bool ForceRegenerate { get; set; }
}

public sealed class GeneratePayslipPdfsForPayrollRunBody
{
    public bool ForceRegenerate { get; set; }
}

public sealed class SendPayslipEmailBody
{
    public string? ToEmail { get; set; }
}
