using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateCandidateApplication;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.MarkCandidateApplicationHired;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.MoveCandidateApplicationToInterview;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.MoveCandidateApplicationToOffer;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.MoveCandidateApplicationToScreening;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.RejectCandidateApplication;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.WithdrawCandidateApplication;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetCandidateApplicationById;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetCandidateApplicationStageHistory;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.SearchCandidateApplications;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Api.Controllers.Recruitment;

[ApiController]
[Route("api/hr/recruitment/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class RecruitmentApplicationsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentManage)]
    [ProducesResponseType(typeof(CandidateApplicationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateCandidateApplication(
        [FromBody] CreateCandidateApplicationCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CreatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.RecruitmentView)]
    [ProducesResponseType(typeof(CandidateApplicationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCandidateApplicationById(
        [FromRoute] CandidateApplicationId id,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetCandidateApplicationByIdQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.RecruitmentView)]
    [ProducesResponseType(typeof(PaginationResponse<CandidateApplicationResponse>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<CandidateApplicationResponse>> SearchCandidateApplications(
        [FromQuery] SearchCandidateApplicationsQuery query,
        CancellationToken cancellationToken)
    {
        return await sender.Send(query, cancellationToken);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentManage)]
    [ProducesResponseType(typeof(CandidateApplicationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> MoveCandidateApplicationToScreening(
        [FromBody] MoveCandidateApplicationToScreeningCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { ChangedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentManage)]
    [ProducesResponseType(typeof(CandidateApplicationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> MoveCandidateApplicationToInterview(
        [FromBody] MoveCandidateApplicationToInterviewCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { ChangedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentManage)]
    [ProducesResponseType(typeof(CandidateApplicationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> MoveCandidateApplicationToOffer(
        [FromBody] MoveCandidateApplicationToOfferCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { ChangedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentManage)]
    [ProducesResponseType(typeof(CandidateApplicationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkCandidateApplicationHired(
        [FromBody] MarkCandidateApplicationHiredCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { ChangedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentManage)]
    [ProducesResponseType(typeof(CandidateApplicationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RejectCandidateApplication(
        [FromBody] RejectCandidateApplicationCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { ChangedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentManage)]
    [ProducesResponseType(typeof(CandidateApplicationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> WithdrawCandidateApplication(
        [FromBody] WithdrawCandidateApplicationCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { ChangedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.RecruitmentView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<CandidateApplicationStageHistoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCandidateApplicationStageHistory(
        [FromRoute] CandidateApplicationId id,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetCandidateApplicationStageHistoryQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}
