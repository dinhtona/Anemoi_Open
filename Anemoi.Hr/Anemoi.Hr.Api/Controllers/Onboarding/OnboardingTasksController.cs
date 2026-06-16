using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.CompleteOnboardingTask;
using Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.ReassignOnboardingTask;
using Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.ReopenOnboardingTask;
using Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.SkipOnboardingTask;
using Anemoi.Hr.Application.Cqrs.Queries.OnboardingQueries.GetMyOnboardingTasks;
using Anemoi.Hr.Application.Cqrs.Queries.OnboardingQueries.GetOnboardingDashboard;
using Anemoi.Hr.Application.Cqrs.Queries.OnboardingQueries.GetPendingOnboardingTasks;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Api.Controllers.Onboarding;

[ApiController]
[Route("api/hr/onboarding/tasks")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class OnboardingTasksController(ISender sender) : ControllerBase
{
    [HttpPost("{id}/complete")]
    [HasPermission(HrPermissions.OnboardingTaskComplete)]
    [ProducesResponseType(typeof(OnboardingInstanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Complete(
        [FromRoute] OnboardingTaskId id,
        [FromBody] CompleteOnboardingTaskCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(
            command with { TaskId = id, CompletedBy = HttpContext.GetUserId() },
            cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}/reopen")]
    [HasPermission(HrPermissions.OnboardingTaskManage)]
    [ProducesResponseType(typeof(OnboardingInstanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reopen(
        [FromRoute] OnboardingTaskId id,
        [FromBody] ReopenOnboardingTaskCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(
            command with { TaskId = id, ReopenedBy = HttpContext.GetUserId() },
            cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}/skip")]
    [HasPermission(HrPermissions.OnboardingTaskComplete)]
    [ProducesResponseType(typeof(OnboardingInstanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Skip(
        [FromRoute] OnboardingTaskId id,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(
            new SkipOnboardingTaskCommand(id) { SkippedBy = HttpContext.GetUserId() },
            cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}/reassign")]
    [HasPermission(HrPermissions.OnboardingTaskManage)]
    [ProducesResponseType(typeof(OnboardingInstanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reassign(
        [FromRoute] OnboardingTaskId id,
        [FromBody] ReassignOnboardingTaskCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(
            command with { TaskId = id, ReassignedBy = HttpContext.GetUserId() },
            cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("~/api/hr/onboarding/my-tasks")]
    [HasPermission(HrPermissions.OnboardingView)]
    [ProducesResponseType(typeof(PaginationResponse<OnboardingTaskResponse>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<OnboardingTaskResponse>> GetMyTasks(
        [FromQuery] GetMyOnboardingTasksQuery query,
        CancellationToken cancellationToken)
    {
        return await sender.Send(
            query with { UserId = HttpContext.GetUserId() },
            cancellationToken);
    }

    [HttpGet("~/api/hr/onboarding/pending-tasks")]
    [HasPermission(HrPermissions.OnboardingView)]
    [ProducesResponseType(typeof(PaginationResponse<OnboardingTaskResponse>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<OnboardingTaskResponse>> GetPendingTasks(
        [FromQuery] GetPendingOnboardingTasksQuery query,
        CancellationToken cancellationToken)
    {
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("~/api/hr/onboarding/dashboard")]
    [HasPermission(HrPermissions.OnboardingView)]
    [ProducesResponseType(typeof(OnboardingDashboardResponse), StatusCodes.Status200OK)]
    public async Task<OnboardingDashboardResponse> GetDashboard(
        CancellationToken cancellationToken)
    {
        return await sender.Send(new GetOnboardingDashboardQuery(), cancellationToken);
    }
}
