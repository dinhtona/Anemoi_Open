#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using Anemoi.Centralize.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Centralize.Api.Controllers.Mock;

[ApiController]
[Route("api/mock")]
public sealed class MockApiController(IMockRouteRepository routeRepository) : ControllerBase
{
    [Route("{*path}")]
    [HttpGet]
    [HttpPost]
    [HttpPut]
    [HttpDelete]
    [HttpPatch]
    [HttpHead]
    [HttpOptions]
    public async Task<IActionResult> HandleMockRequest([FromRoute] string? path)
    {
        var method = Request.Method;
        var matchedPath = path ?? "";

        var route = await routeRepository.GetMatchingRouteAsync(matchedPath, method);
        if (route == null)
        {
            return NotFound(new
            {
                error = "Mock route not found.",
                method = method,
                path = matchedPath,
                message = $"No active mock response configured for {method} /{matchedPath}. Define it in the developer dashboard Environments tab."
            });
        }

        // Return configured content type, status code, and response body
        var contentResult = Content(route.ResponseBody, route.ContentType ?? "application/json", System.Text.Encoding.UTF8);
        contentResult.StatusCode = route.StatusCode;
        return contentResult;
    }
}
