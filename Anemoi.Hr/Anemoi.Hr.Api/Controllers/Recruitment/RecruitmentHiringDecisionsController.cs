using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateHireDecision;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateOfferDecision;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateRejectDecision;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetHiringDecisionById;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.SearchHiringDecisions;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Api.Controllers.Recruitment;

[ApiController]
[Route("api/hr/recruitment/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class RecruitmentHiringDecisionsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentHire)]
    [ProducesResponseType(typeof(HiringDecisionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateOfferDecision(
        [FromBody] CreateOfferDecisionCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { DecidedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentHire)]
    [ProducesResponseType(typeof(HiringDecisionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateHireDecision(
        [FromBody] CreateHireDecisionCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { DecidedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentHire)]
    [ProducesResponseType(typeof(HiringDecisionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateRejectDecision(
        [FromBody] CreateRejectDecisionCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { DecidedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.RecruitmentView)]
    [ProducesResponseType(typeof(HiringDecisionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHiringDecisionById(
        [FromRoute] HiringDecisionId id,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetHiringDecisionByIdQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.RecruitmentView)]
    [ProducesResponseType(typeof(PaginationResponse<HiringDecisionResponse>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<HiringDecisionResponse>> SearchHiringDecisions(
        [FromQuery] SearchHiringDecisionsQuery query,
        CancellationToken cancellationToken)
    {
        return await sender.Send(query, cancellationToken);
    }
}
