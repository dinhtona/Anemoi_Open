using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.CancelOnboarding;
using Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.ForceCompleteOnboarding;
using Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.ReopenOnboarding;
using Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.StartOnboarding;
using Anemoi.Hr.Application.Cqrs.Queries.OnboardingQueries.GetEmployeeOnboarding;
using Anemoi.Hr.Application.Cqrs.Queries.OnboardingQueries.GetOnboardingInstanceById;
using Anemoi.Hr.Application.Cqrs.Queries.OnboardingQueries.GetOnboardingInstances;
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
[Route("api/hr/onboarding/instances")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class OnboardingInstancesController(ISender sender) : ControllerBase
{
    [HttpPost("start")]
    [HasPermission(HrPermissions.OnboardingManage)]
    [ProducesResponseType(typeof(OnboardingInstanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Start(
        [FromBody] StartOnboardingCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CreatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.OnboardingView)]
    [ProducesResponseType(typeof(PaginationResponse<OnboardingInstanceResponse>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<OnboardingInstanceResponse>> GetAll(
        [FromQuery] GetOnboardingInstancesQuery query,
        CancellationToken cancellationToken)
    {
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.OnboardingView)]
    [ProducesResponseType(typeof(OnboardingInstanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(
        [FromRoute] OnboardingInstanceId id,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetOnboardingInstanceByIdQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}/cancel")]
    [HasPermission(HrPermissions.OnboardingManage)]
    [ProducesResponseType(typeof(OnboardingInstanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Cancel(
        [FromRoute] OnboardingInstanceId id,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(
            new CancelOnboardingCommand(id) { CancelledBy = HttpContext.GetUserId() },
            cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}/reopen")]
    [HasPermission(HrPermissions.OnboardingManage)]
    [ProducesResponseType(typeof(OnboardingInstanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reopen(
        [FromRoute] OnboardingInstanceId id,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(
            new ReopenOnboardingCommand(id) { ReopenedBy = HttpContext.GetUserId() },
            cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}/force-complete")]
    [HasPermission(HrPermissions.OnboardingManage)]
    [ProducesResponseType(typeof(OnboardingInstanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForceComplete(
        [FromRoute] OnboardingInstanceId id,
        [FromBody] ForceCompleteOnboardingCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(
            command with { InstanceId = id, CompletedBy = HttpContext.GetUserId() },
            cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("~/api/hr/employees/{employeeId}/onboarding")]
    [HasPermission(HrPermissions.OnboardingView)]
    [ProducesResponseType(typeof(OnboardingInstanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeOnboarding(
        [FromRoute] EmployeeId employeeId,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetEmployeeOnboardingQuery(employeeId), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}
