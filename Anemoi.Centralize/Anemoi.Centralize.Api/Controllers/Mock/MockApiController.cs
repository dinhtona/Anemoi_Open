#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.Centralize.Application.Abstractions;
using Anemoi.Centralize.Application.Cqrs.Environments.Queries;
using Anemoi.BuildingBlock.Application.Resources;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Anemoi.Centralize.Api.Controllers.Mock;

[ApiController]
[Route("api/mock")]
[Authorize]
public sealed class MockApiController(
    ISender sender,
    IStringLocalizer<SharedResource> localizer) : ControllerBase
{
    [Route("{*path}")]
    [HttpGet]
    [HttpPost]
    [HttpPut]
    [HttpDelete]
    [HttpPatch]
    [HttpHead]
    [HttpOptions]
    public async Task<IActionResult> HandleMockRequest([FromRoute] string? path, CancellationToken cancellationToken)
    {
        var method = Request.Method;
        var matchedPath = path ?? "";

        var route = await sender.Send(new GetMatchingMockRouteQuery(matchedPath, method), cancellationToken);
        if (route == null)
        {
            return NotFound(new
            {
                error = localizer["MockRouteNotFound"].Value,
                method = method,
                path = matchedPath,
                message = localizer["MockRouteNotConfigured", method, matchedPath].Value
            });
        }

        // Return configured content type, status code, and response body
        var contentResult = Content(route.ResponseBody, route.ContentType ?? "application/json", System.Text.Encoding.UTF8);
        contentResult.StatusCode = route.StatusCode;
        return contentResult;
    }
}
