using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.LeaveBalanceCommands.AdjustLeaveBalance;
using Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.ApproveLeaveRequest;
using Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.CancelLeaveRequest;
using Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.ForceApproveLeaveRequest;
using Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.ForceCancelLeaveRequest;
using Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.RejectLeaveRequest;
using Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.SubmitLeaveRequest;
using Anemoi.Hr.Application.Cqrs.Queries.LeaveBalanceQueries.GetLeaveBalance;
using Anemoi.Hr.Application.Cqrs.Queries.LeaveBalanceQueries.GetLeaveBalances;
using Anemoi.Hr.Application.Cqrs.Queries.LeaveRequestQueries.GetLeaveRequest;
using Anemoi.Hr.Application.Cqrs.Queries.LeaveRequestQueries.GetLeaveRequests;
using Anemoi.Hr.Application.Cqrs.Queries.LeaveTransactionQueries.GetLeaveTransactions;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.Leave;

[ApiController]
[Route("api/hr/leave/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class LeaveController(ISender sender) : ControllerBase
{
    [HttpGet("{id}")]
    [HasPermission(HrPermissions.LeaveBalanceView)]
    [ProducesResponseType(typeof(LeaveBalanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaveBalance([FromRoute] LeaveBalanceId id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetLeaveBalanceQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.LeaveBalanceView)]
    [ProducesResponseType(typeof(PaginationResponse<LeaveBalanceResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaveBalances([FromQuery] GetLeaveBalancesQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(query, cancellationToken));
    }

    [HttpPost]
    [HasPermission(HrPermissions.LeaveBalanceAdjust)]
    public async Task<IActionResult> AdjustLeaveBalance([FromBody] AdjustLeaveBalanceCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.LeaveRequestView)]
    [ProducesResponseType(typeof(LeaveRequestResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaveRequest([FromRoute] LeaveRequestId id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetLeaveRequestQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.LeaveRequestView)]
    [ProducesResponseType(typeof(PaginationResponse<LeaveRequestResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaveRequests([FromQuery] GetLeaveRequestsQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(query, cancellationToken));
    }

    [HttpPost]
    [HasPermission(HrPermissions.LeaveRequestCreate)]
    [ProducesResponseType(typeof(LeaveRequestIdResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SubmitLeaveRequest([FromBody] SubmitLeaveRequestCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}")]
    [HasPermission(HrPermissions.LeaveRequestApprove)]
    public async Task<IActionResult> ApproveLeaveRequest([FromRoute] LeaveRequestId id,
        [FromBody] ApproveLeaveRequestCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { Id = id }, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPost("{id}")]
    [HasPermission(HrPermissions.LeaveRequestApprove)]
    public async Task<IActionResult> RejectLeaveRequest([FromRoute] LeaveRequestId id,
        [FromBody] RejectLeaveRequestCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { Id = id }, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPost("{id}")]
    [HasPermission(HrPermissions.LeaveRequestCancel)]
    public async Task<IActionResult> CancelLeaveRequest([FromRoute] LeaveRequestId id,
        [FromBody] CancelLeaveRequestCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { Id = id }, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPost("{id}")]
    [HasPermission(HrPermissions.LeaveRequestForceApprove)]
    public async Task<IActionResult> ForceApproveLeaveRequest([FromRoute] LeaveRequestId id,
        [FromBody] ForceApproveLeaveRequestCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { Id = id }, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPost("{id}")]
    [HasPermission(HrPermissions.LeaveRequestForceCancel)]
    public async Task<IActionResult> ForceCancelLeaveRequest([FromRoute] LeaveRequestId id,
        [FromBody] ForceCancelLeaveRequestCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { Id = id }, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.LeaveTransactionView)]
    [ProducesResponseType(typeof(PaginationResponse<LeaveTransactionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaveTransactions([FromQuery] GetLeaveTransactionsQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(query, cancellationToken));
    }
}
