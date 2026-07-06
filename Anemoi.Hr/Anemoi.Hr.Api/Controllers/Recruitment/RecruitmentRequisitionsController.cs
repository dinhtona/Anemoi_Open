using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.ApproveRequisition;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CancelRequisition;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CloseRequisition;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateRequisition;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.RejectRequisition;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.SubmitRequisition;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.UpdateRequisition;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRequisitionById;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.SearchRequisitions;
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
public sealed class RecruitmentRequisitionsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentManage)]
    [ProducesResponseType(typeof(JobRequisitionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateRequisition(
        [FromBody] CreateRequisitionCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CreatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentManage)]
    [ProducesResponseType(typeof(JobRequisitionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateRequisition(
        [FromBody] UpdateRequisitionCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { UpdatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentManage)]
    [ProducesResponseType(typeof(JobRequisitionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SubmitRequisition(
        [FromBody] SubmitRequisitionCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { SubmittedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentManage)]
    [ProducesResponseType(typeof(JobRequisitionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ApproveRequisition(
        [FromBody] ApproveRequisitionCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { ApprovedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentManage)]
    [ProducesResponseType(typeof(JobRequisitionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RejectRequisition(
        [FromBody] RejectRequisitionCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { RejectedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentManage)]
    [ProducesResponseType(typeof(JobRequisitionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CloseRequisition(
        [FromBody] CloseRequisitionCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { ClosedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.RecruitmentManage)]
    [ProducesResponseType(typeof(JobRequisitionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelRequisition(
        [FromBody] CancelRequisitionCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CancelledBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.RecruitmentView)]
    [ProducesResponseType(typeof(JobRequisitionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRequisitionById(
        [FromRoute] JobRequisitionId id,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetRequisitionByIdQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.RecruitmentView)]
    [ProducesResponseType(typeof(PaginationResponse<JobRequisitionResponse>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<JobRequisitionResponse>> SearchRequisitions(
        [FromQuery] SearchRequisitionsQuery query,
        CancellationToken cancellationToken)
    {
        return await sender.Send(query, cancellationToken);
    }
}
