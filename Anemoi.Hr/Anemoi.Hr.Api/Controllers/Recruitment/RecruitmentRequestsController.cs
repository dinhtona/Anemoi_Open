using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.ApproveRecruitmentRequest;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CancelRecruitmentRequest;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateRecruitmentRequest;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.RejectRecruitmentRequest;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.SubmitRecruitmentRequest;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.UpdateRecruitmentRequest;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentRequestById;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentRequests;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentRequestTimeline;
using Anemoi.Hr.Application.Responses;
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
public sealed class RecruitmentRequestsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentRequestCreate)]
    [ProducesResponseType(typeof(RecruitmentRequestResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateRecruitmentRequest(
        [FromBody] CreateRecruitmentRequestCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CreatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentRequestCreate)]
    [ProducesResponseType(typeof(RecruitmentRequestResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateRecruitmentRequest(
        [FromBody] UpdateRecruitmentRequestCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { UpdatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentRequestSubmit)]
    [ProducesResponseType(typeof(RecruitmentRequestResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SubmitRecruitmentRequest(
        [FromBody] SubmitRecruitmentRequestCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { SubmittedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentRequestApprove)]
    [ProducesResponseType(typeof(RecruitmentRequestResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ApproveRecruitmentRequest(
        [FromBody] ApproveRecruitmentRequestCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { ApprovedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentRequestApprove)]
    [ProducesResponseType(typeof(RecruitmentRequestResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RejectRecruitmentRequest(
        [FromBody] RejectRecruitmentRequestCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { RejectedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentRequestManage)]
    [ProducesResponseType(typeof(RecruitmentRequestResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelRecruitmentRequest(
        [FromBody] CancelRecruitmentRequestCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CancelledBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.RecruitmentRequestView)]
    [ProducesResponseType(typeof(RecruitmentRequestResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecruitmentRequestById(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetRecruitmentRequestByIdQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.RecruitmentRequestView)]
    [ProducesResponseType(typeof(PaginationResponse<RecruitmentRequestResponse>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<RecruitmentRequestResponse>> GetRecruitmentRequests(
        [FromQuery] GetRecruitmentRequestsQuery query,
        CancellationToken cancellationToken)
    {
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("{id}/timeline")]
    [HasPermission(HrPermissions.RecruitmentRequestView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<RecruitmentRequestHistoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecruitmentRequestTimeline(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetRecruitmentRequestTimelineQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}
