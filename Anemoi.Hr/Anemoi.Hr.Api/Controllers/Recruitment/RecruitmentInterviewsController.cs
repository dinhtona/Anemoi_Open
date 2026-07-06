using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateInterviewSchedule;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.MarkInterviewFailed;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.MarkInterviewNoShow;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.MarkInterviewPassed;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.RescheduleInterview;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.SubmitInterviewFeedback;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetInterviewById;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetInterviewFeedbacks;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.SearchInterviews;
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
public sealed class RecruitmentInterviewsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentInterview)]
    [ProducesResponseType(typeof(InterviewScheduleResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateInterviewSchedule(
        [FromBody] CreateInterviewScheduleCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CreatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentInterview)]
    [ProducesResponseType(typeof(InterviewScheduleResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RescheduleInterview(
        [FromBody] RescheduleInterviewCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { UpdatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentInterview)]
    [ProducesResponseType(typeof(InterviewScheduleResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkInterviewPassed(
        [FromBody] MarkInterviewPassedCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { UpdatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentInterview)]
    [ProducesResponseType(typeof(InterviewScheduleResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkInterviewFailed(
        [FromBody] MarkInterviewFailedCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { UpdatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentInterview)]
    [ProducesResponseType(typeof(InterviewScheduleResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkInterviewNoShow(
        [FromBody] MarkInterviewNoShowCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { UpdatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentInterview)]
    [ProducesResponseType(typeof(InterviewFeedbackResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SubmitInterviewFeedback(
        [FromBody] SubmitInterviewFeedbackCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with
            { InterviewerEmployeeId = new EmployeeId(System.Guid.Parse(HttpContext.GetUserId())) },
            cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.RecruitmentView)]
    [ProducesResponseType(typeof(InterviewScheduleResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInterviewById(
        [FromRoute] InterviewScheduleId id,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetInterviewByIdQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.RecruitmentView)]
    [ProducesResponseType(typeof(PaginationResponse<InterviewScheduleResponse>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<InterviewScheduleResponse>> SearchInterviews(
        [FromQuery] SearchInterviewsQuery query,
        CancellationToken cancellationToken)
    {
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.RecruitmentView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<InterviewFeedbackResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInterviewFeedbacks(
        [FromRoute] InterviewScheduleId id,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetInterviewFeedbacksQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}
