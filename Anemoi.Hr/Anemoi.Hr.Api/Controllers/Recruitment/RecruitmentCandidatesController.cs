using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.ArchiveCandidate;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.BlacklistCandidate;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.ChangeCandidateSource;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateCandidate;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.ReactivateCandidate;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.UpdateCandidate;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetCandidateById;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.SearchCandidates;
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
public sealed class RecruitmentCandidatesController(ISender sender) : ControllerBase
{
    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentManage)]
    [ProducesResponseType(typeof(CandidateResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateCandidate(
        [FromBody] CreateCandidateCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CreatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentManage)]
    [ProducesResponseType(typeof(CandidateResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateCandidate(
        [FromBody] UpdateCandidateCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { UpdatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentManage)]
    [ProducesResponseType(typeof(CandidateResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangeCandidateSource(
        [FromBody] ChangeCandidateSourceCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { UpdatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentManage)]
    [ProducesResponseType(typeof(CandidateResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> BlacklistCandidate(
        [FromBody] BlacklistCandidateCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { UpdatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentManage)]
    [ProducesResponseType(typeof(CandidateResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ArchiveCandidate(
        [FromBody] ArchiveCandidateCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { UpdatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentManage)]
    [ProducesResponseType(typeof(CandidateResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReactivateCandidate(
        [FromBody] ReactivateCandidateCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { UpdatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.RecruitmentView)]
    [ProducesResponseType(typeof(CandidateResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCandidateById(
        [FromRoute] CandidateId id,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetCandidateByIdQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.RecruitmentView)]
    [ProducesResponseType(typeof(PaginationResponse<CandidateResponse>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<CandidateResponse>> SearchCandidates(
        [FromQuery] SearchCandidatesQuery query,
        CancellationToken cancellationToken)
    {
        return await sender.Send(query, cancellationToken);
    }
}
