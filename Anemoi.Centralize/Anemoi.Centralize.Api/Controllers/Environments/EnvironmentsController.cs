#nullable enable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.Centralize.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Centralize.Application.Cqrs.Environments.Commands;
using Anemoi.Centralize.Application.Cqrs.Environments.Queries;
using Anemoi.Centralize.Domain.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Centralize.Api.Controllers.Environments;

[ApiController]
[Route("api/environments")]
public sealed class EnvironmentsController(
    ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetEnvironments(CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetEnvironmentsQuery(), cancellationToken);
        return Ok(new { data = res });
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> StartEnvironment([FromRoute] string id, CancellationToken cancellationToken)
    {
        var success = await sender.Send(new StartEnvironmentCommand(id), cancellationToken);
        if (!success)
        {
            return BadRequest(CreateError("FailedToStartEnvironment"));
        }
        var environments = await sender.Send(new GetEnvironmentsQuery(), cancellationToken);
        var updated = environments.Find(e =>
            e.Id == id ||
            (id == "mail" && e.Id == "smtp4dev_server") ||
            (id == "sftp" && e.Id == "sftp_server")
        );
        return Ok(new { data = updated });
    }

    [HttpPost("{id}/stop")]
    public async Task<IActionResult> StopEnvironment([FromRoute] string id, CancellationToken cancellationToken)
    {
        var success = await sender.Send(new StopEnvironmentCommand(id), cancellationToken);
        if (!success)
        {
            return BadRequest(CreateError("FailedToStopEnvironment"));
        }
        var environments = await sender.Send(new GetEnvironmentsQuery(), cancellationToken);
        var updated = environments.Find(e =>
            e.Id == id ||
            (id == "mail" && e.Id == "smtp4dev_server") ||
            (id == "sftp" && e.Id == "sftp_server")
        );
        return Ok(new { data = updated });
    }

    [HttpGet("{id}/status")]
    public async Task<IActionResult> GetEnvironmentStatus([FromRoute] string id, CancellationToken cancellationToken)
    {
        var environments = await sender.Send(new GetEnvironmentsQuery(), cancellationToken);
        var updated = environments.Find(e =>
            e.Id == id ||
            (id == "mail" && e.Id == "smtp4dev_server") ||
            (id == "sftp" && e.Id == "sftp_server")
        );
        if (updated == null)
        {
            return NotFound(CreateError("EnvironmentNotFound"));
        }
        return Ok(new { data = updated });
    }

    // --- SFTP File Endpoints ---

    [HttpGet("sftp/files")]
    public async Task<IActionResult> GetSftpFiles([FromQuery] string? path, CancellationToken cancellationToken)
    {
        var files = await sender.Send(new GetSftpFilesQuery(path), cancellationToken);
        return Ok(new { data = files });
    }

    [HttpPost("sftp/upload")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UploadSftpFile(
        [FromQuery] string? path,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken
    )
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(CreateError("NoFileUploaded"));
        }

        using var stream = file.OpenReadStream();
        var success = await sender.Send(new UploadSftpFileCommand(path, file.FileName, stream), cancellationToken);
        return Ok(new { success });
    }

    [HttpGet("sftp/download")]
    public async Task<IActionResult> DownloadSftpFile([FromQuery] string path, CancellationToken cancellationToken)
    {
        try
        {
            var response = await sender.Send(new DownloadSftpFileQuery(path), cancellationToken);
            return File(response.Stream, response.ContentType, response.FileName, enableRangeProcessing: true);
        }
        catch (FileNotFoundException)
        {
            return NotFound(CreateError("FileNotFound"));
        }
        catch (Exception)
        {
            return BadRequest(CreateError("FileOperationFailed"));
        }
    }

    [HttpDelete("sftp/files")]
    public async Task<IActionResult> DeleteSftpFile([FromQuery] string path, CancellationToken cancellationToken)
    {
        var success = await sender.Send(new DeleteSftpFileCommand(path), cancellationToken);
        return Ok(new { success });
    }

    [HttpPost("sftp/directory")]
    public async Task<IActionResult> CreateSftpDirectory([FromQuery] string path, CancellationToken cancellationToken)
    {
        var success = await sender.Send(new CreateSftpDirectoryCommand(path), cancellationToken);
        return Ok(new { success });
    }

    // --- Mock API Route Endpoints ---

    [HttpGet("api-test/routes")]
    public async Task<IActionResult> GetMockRoutes(CancellationToken cancellationToken)
    {
        var routes = await sender.Send(new GetMockRoutesQuery(), cancellationToken);
        return Ok(new { data = routes });
    }

    [HttpPost("api-test/routes")]
    public async Task<IActionResult> CreateMockRoute(
        [FromBody] CreateMockRouteCommand command,
        CancellationToken cancellationToken
    )
    {
        var route = await sender.Send(command, cancellationToken);
        return Ok(new { data = route });
    }

    [HttpPut("api-test/routes")]
    public async Task<IActionResult> UpdateMockRoute(
        [FromBody] UpdateMockRouteCommand command,
        CancellationToken cancellationToken
    )
    {
        var route = await sender.Send(command, cancellationToken);
        if (route == null)
        {
            return NotFound(CreateError("MockRouteNotFound"));
        }
        return Ok(new { data = route });
    }

    [HttpDelete("api-test/routes/{id}")]
    public async Task<IActionResult> DeleteMockRoute([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var success = await sender.Send(new DeleteMockRouteCommand(new MockRouteId(id)), cancellationToken);
        return Ok(new { success });
    }

    private static ErrorDetailResponse CreateError(string code) => new()
    {
        Code = code,
        Messages = [code]
    };
}
