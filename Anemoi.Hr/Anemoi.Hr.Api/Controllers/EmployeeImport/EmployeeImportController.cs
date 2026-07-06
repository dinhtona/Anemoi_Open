using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.BulkImport.Models;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.EmployeeImport;
using Anemoi.Hr.Application.Cqrs.Queries.EmployeeImport;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.EmployeeImport;

[ApiController]
[Route("api/hr/employees/imports")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class EmployeeImportController(ISender sender) : ControllerBase
{
    [HttpPost]
    [HasPermission(HrPermissions.EmployeeImport)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(EmployeeImportPreviewResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadImportFile(
        IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length is 0)
            return BadRequest("No file uploaded");

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, cancellationToken);
        var command = new UploadImportFileCommand(
            file.FileName, file.Length, ms.ToArray());
        var result = await sender.Send(command, cancellationToken);
        return result.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{jobId}/preview")]
    [HasPermission(HrPermissions.EmployeeImport)]
    [ProducesResponseType(typeof(EmployeeImportPreviewResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> PreviewImport(
        Guid jobId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new PreviewImportCommand(new BulkImportJobId(jobId)), cancellationToken);
        return result.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{jobId}/execute")]
    [HasPermission(HrPermissions.EmployeeImport)]
    [ProducesResponseType(typeof(EmployeeImportResultResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExecuteImport(
        Guid jobId, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var result = await sender.Send(
            new ExecuteImportCommand(new BulkImportJobId(jobId), userId, HttpContext.GetClaimValue("name")),
            cancellationToken);
        return result.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.EmployeeImportHistory)]
    [ProducesResponseType(typeof(PaginationResponse<EmployeeImportHistoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetImportHistory(
        [FromQuery] GetImportHistoryQuery query, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(query, cancellationToken));
    }

    [HttpGet("{jobId}")]
    [HasPermission(HrPermissions.EmployeeImportHistory)]
    [ProducesResponseType(typeof(EmployeeImportJobDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetImportDetail(
        Guid jobId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetImportJobDetailQuery(new BulkImportJobId(jobId)), cancellationToken);
        return result.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("template")]
    [HasPermission(HrPermissions.EmployeeImportTemplate)]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> DownloadTemplate(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetImportTemplateQuery(), cancellationToken);
        return result.Match<IActionResult>(
            download => File(download.FileBytes, download.ContentType, download.FileName),
            BadRequest);
    }

    [HttpGet("{jobId}/error-file")]
    [HasPermission(HrPermissions.EmployeeImportDownload)]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> DownloadErrorFile(
        Guid jobId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DownloadErrorFileQuery(new BulkImportJobId(jobId)), cancellationToken);
        return result.Match<IActionResult>(
            download => File(download.FileBytes, download.ContentType, download.FileName),
            BadRequest);
    }
}
