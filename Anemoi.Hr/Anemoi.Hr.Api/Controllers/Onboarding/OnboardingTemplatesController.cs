using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.ActivateOnboardingPlanTemplate;
using Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.CreateOnboardingPlanTemplate;
using Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.DeactivateOnboardingPlanTemplate;
using Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.UpdateOnboardingPlanTemplate;
using Anemoi.Hr.Application.Cqrs.Queries.OnboardingQueries.GetOnboardingPlanTemplateById;
using Anemoi.Hr.Application.Cqrs.Queries.OnboardingQueries.GetOnboardingPlanTemplates;
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
[Route("api/hr/onboarding/templates")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class OnboardingTemplatesController(ISender sender) : ControllerBase
{
    [HttpPost]
    [HasPermission(HrPermissions.OnboardingManage)]
    [ProducesResponseType(typeof(OnboardingPlanTemplateResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(
        [FromBody] CreateOnboardingPlanTemplateCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CreatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPut("{id}")]
    [HasPermission(HrPermissions.OnboardingManage)]
    [ProducesResponseType(typeof(OnboardingPlanTemplateResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        [FromRoute] OnboardingPlanTemplateId id,
        [FromBody] UpdateOnboardingPlanTemplateCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { Id = id, UpdatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}/activate")]
    [HasPermission(HrPermissions.OnboardingManage)]
    [ProducesResponseType(typeof(OnboardingPlanTemplateResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Activate(
        [FromRoute] OnboardingPlanTemplateId id,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(
            new ActivateOnboardingPlanTemplateCommand(id) { UpdatedBy = HttpContext.GetUserId() },
            cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}/deactivate")]
    [HasPermission(HrPermissions.OnboardingManage)]
    [ProducesResponseType(typeof(OnboardingPlanTemplateResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Deactivate(
        [FromRoute] OnboardingPlanTemplateId id,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(
            new DeactivateOnboardingPlanTemplateCommand(id) { UpdatedBy = HttpContext.GetUserId() },
            cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.OnboardingView)]
    [ProducesResponseType(typeof(OnboardingPlanTemplateResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(
        [FromRoute] OnboardingPlanTemplateId id,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetOnboardingPlanTemplateByIdQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.OnboardingView)]
    [ProducesResponseType(typeof(PaginationResponse<OnboardingPlanTemplateResponse>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<OnboardingPlanTemplateResponse>> GetAll(
        [FromQuery] GetOnboardingPlanTemplatesQuery query,
        CancellationToken cancellationToken)
    {
        return await sender.Send(query, cancellationToken);
    }
}
