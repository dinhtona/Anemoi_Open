using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CloseJobPosting;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateJobPosting;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.ExpireJobPosting;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.PublishJobPosting;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.UpdateJobPosting;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetJobPostingById;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.SearchJobPostings;
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
public sealed class RecruitmentJobPostingsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentManage)]
    [ProducesResponseType(typeof(JobPostingResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateJobPosting(
        [FromBody] CreateJobPostingCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CreatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentManage)]
    [ProducesResponseType(typeof(JobPostingResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateJobPosting(
        [FromBody] UpdateJobPostingCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { UpdatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentManage)]
    [ProducesResponseType(typeof(JobPostingResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> PublishJobPosting(
        [FromBody] PublishJobPostingCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { PublishedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentManage)]
    [ProducesResponseType(typeof(JobPostingResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExpireJobPosting(
        [FromBody] ExpireJobPostingCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { ExpiredBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentManage)]
    [ProducesResponseType(typeof(JobPostingResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CloseJobPosting(
        [FromBody] CloseJobPostingCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { ClosedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.RecruitmentView)]
    [ProducesResponseType(typeof(JobPostingResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetJobPostingById(
        [FromRoute] JobPostingId id,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetJobPostingByIdQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.RecruitmentView)]
    [ProducesResponseType(typeof(PaginationResponse<JobPostingResponse>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<JobPostingResponse>> SearchJobPostings(
        [FromQuery] SearchJobPostingsQuery query,
        CancellationToken cancellationToken)
    {
        return await sender.Send(query, cancellationToken);
    }
}
