#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.Centralize.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Authorization;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Centralize.Application.Cqrs.Environments.Commands;
using Anemoi.Centralize.Application.Cqrs.Environments.Queries;
using Anemoi.Centralize.Domain.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Anemoi.Centralize.Api.Controllers.Environments;

[ApiController]
[Route("api/environments")]
[Authorize(Policy = AuthorizationPolicies.Internal)]
[EnableRateLimiting("general-limit")]
public sealed class EnvironmentsController(
    ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.EnvironmentRead)]
    public async Task<IActionResult> GetEnvironments(CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetEnvironmentsQuery(), cancellationToken);
        return Ok(new { data = res });
    }

    [HttpPost("{id}/start")]
    [HasPermission(Permissions.EnvironmentStartStop)]
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
            (id == "mail" && e.Id == "smtp4dev_server")
        );
        return Ok(new { data = updated });
    }

    [HttpPost("{id}/stop")]
    [HasPermission(Permissions.EnvironmentStartStop)]
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
            (id == "mail" && e.Id == "smtp4dev_server")
        );
        return Ok(new { data = updated });
    }

    [HttpGet("{id}/status")]
    [HasPermission(Permissions.EnvironmentRead)]
    public async Task<IActionResult> GetEnvironmentStatus([FromRoute] string id, CancellationToken cancellationToken)
    {
        var environments = await sender.Send(new GetEnvironmentsQuery(), cancellationToken);
        var updated = environments.Find(e =>
            e.Id == id ||
            (id == "mail" && e.Id == "smtp4dev_server")
        );
        if (updated == null)
        {
            return NotFound(CreateError("EnvironmentNotFound"));
        }
        return Ok(new { data = updated });
    }

    // --- Mock API Route Endpoints ---

    [HttpGet("api-test/routes")]
    [HasPermission(Permissions.EnvironmentMockRouteRead)]
    public async Task<IActionResult> GetMockRoutes(CancellationToken cancellationToken)
    {
        var routes = await sender.Send(new GetMockRoutesQuery(), cancellationToken);
        return Ok(new { data = routes });
    }

    [HttpPost("api-test/routes")]
    [HasPermission(Permissions.EnvironmentMockRouteManage)]
    public async Task<IActionResult> CreateMockRoute(
        [FromBody] CreateMockRouteCommand command,
        CancellationToken cancellationToken
    )
    {
        var route = await sender.Send(command, cancellationToken);
        return Ok(new { data = route });
    }

    [HttpPut("api-test/routes")]
    [HasPermission(Permissions.EnvironmentMockRouteManage)]
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
    [HasPermission(Permissions.EnvironmentMockRouteManage)]
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
