using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Cqrs.Queries.ManagerApprovalQueries.GetMyPendingLeaveApprovals;
using Anemoi.Hr.Application.Cqrs.Queries.ManagerApprovalQueries.GetMyPendingOvertimeApprovals;
using Anemoi.Hr.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.Manager;

[ApiController]
[Route("api/hr/manager/approvals")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class ManagerApprovalsController(ISender sender) : ControllerBase
{
    [HttpGet("leave")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyCollection<ManagerLeavePendingApprovalResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingLeaveApprovals(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var result = await sender.Send(new GetMyPendingLeaveApprovalsQuery(userId), cancellationToken);
        return result.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("overtime")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyCollection<ManagerOvertimePendingApprovalResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingOvertimeApprovals(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var result = await sender.Send(new GetMyPendingOvertimeApprovalsQuery(userId), cancellationToken);
        return result.Match<IActionResult>(Ok, BadRequest);
    }
}
