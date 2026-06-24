#nullable enable
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.EssCommands.CancelMyLeaveRequest;
using Anemoi.Hr.Application.Cqrs.Commands.EssCommands.CancelMyOvertimeRequest;
using Anemoi.Hr.Application.Cqrs.Commands.EssCommands.SubmitMyLeaveRequest;
using Anemoi.Hr.Application.Cqrs.Commands.EssCommands.SubmitMyOvertimeRequest;
using Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyAttendanceRecords;
using Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyAttendanceSummary;
using Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyLeaveBalances;
using Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyApproverPreview;
using Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyLeaveRequests;
using Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyOvertimeRequests;
using Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyPayrollHistory;
using Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyPayslipDetail;
using Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyPayslips;
using Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyProfile;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.Ess;

[ApiController]
[Route("api/hr/ess")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class EssController(ISender sender) : ControllerBase
{
    [HttpGet("profile")]
    [HasPermission(HrPermissions.EssProfileView)]
    [ProducesResponseType(typeof(EssEmployeeProfileResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var email = HttpContext.GetClaimValue("email");
        var res = await sender.Send(new GetMyProfileQuery(userId, email), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("leave/balances")]
    [HasPermission(HrPermissions.EssLeaveView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<EssLeaveBalanceResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyLeaveBalances(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var email = HttpContext.GetClaimValue("email");
        var res = await sender.Send(new GetMyLeaveBalancesQuery(userId, email), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("leave/requests")]
    [HasPermission(HrPermissions.EssLeaveView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<EssLeaveRequestResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyLeaveRequests(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var email = HttpContext.GetClaimValue("email");
        var res = await sender.Send(new GetMyLeaveRequestsQuery(userId, email), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("leave/approver-preview")]
    [HasPermission(HrPermissions.EssLeaveRequest)]
    [ProducesResponseType(typeof(EssApproverPreviewResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyApproverPreview(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var email = HttpContext.GetClaimValue("email");
        var res = await sender.Send(new GetMyApproverPreviewQuery(userId!, email!), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("attendance/records")]
    [HasPermission(HrPermissions.EssAttendanceView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<EssAttendanceRecordResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyAttendanceRecords(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var email = HttpContext.GetClaimValue("email");
        var res = await sender.Send(new GetMyAttendanceRecordsQuery(userId, email), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("attendance/summary")]
    [HasPermission(HrPermissions.EssAttendanceView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<EssAttendanceSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyAttendanceSummary(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var email = HttpContext.GetClaimValue("email");
        var res = await sender.Send(new GetMyAttendanceSummaryQuery(userId, email), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("overtime/requests")]
    [HasPermission(HrPermissions.EssOvertimeView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<EssOvertimeRequestResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOvertimeRequests(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var email = HttpContext.GetClaimValue("email");
        var res = await sender.Send(new GetMyOvertimeRequestsQuery(userId, email), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("payroll/history")]
    [HasPermission(HrPermissions.EssPayrollView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<EssPayrollPeriodResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPayrollHistory(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var email = HttpContext.GetClaimValue("email");
        var res = await sender.Send(new GetMyPayrollHistoryQuery(userId, email), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("payslips")]
    [HasPermission(HrPermissions.EssPayslipView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<EssPayslipResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPayslips(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var email = HttpContext.GetClaimValue("email");
        var res = await sender.Send(new GetMyPayslipsQuery(userId, email), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("payslips/{payslipId}")]
    [HasPermission(HrPermissions.EssPayslipView)]
    [ProducesResponseType(typeof(EssPayslipResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPayslipDetail([FromRoute] PayslipId payslipId, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var email = HttpContext.GetClaimValue("email");
        var res = await sender.Send(new GetMyPayslipDetailQuery(userId, email, payslipId), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("leave/submit")]
    [HasPermission(HrPermissions.EssLeaveRequest)]
    [ProducesResponseType(typeof(LeaveRequestIdResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SubmitMyLeaveRequest([FromBody] SubmitMyLeaveRequestCommand command,
        CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var email = HttpContext.GetClaimValue("email");
        var res = await sender.Send(command with { UserId = userId, Email = email }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("leave/cancel/{leaveRequestId}")]
    [HasPermission(HrPermissions.EssLeaveRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelMyLeaveRequest([FromRoute] LeaveRequestId leaveRequestId,
        [FromBody] CancelMyLeaveRequestBody? body,
        CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var email = HttpContext.GetClaimValue("email");
        var reason = body?.Reason;
        var res = await sender.Send(new CancelMyLeaveRequestCommand(userId, email, leaveRequestId, reason), cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPost("overtime/submit")]
    [HasPermission(HrPermissions.EssOvertimeCreate)]
    [ProducesResponseType(typeof(OvertimeRequestIdResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SubmitMyOvertimeRequest([FromBody] SubmitMyOvertimeRequestCommand command,
        CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var email = HttpContext.GetClaimValue("email");
        var res = await sender.Send(command with { UserId = userId, Email = email }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("overtime/cancel/{overtimeRequestId}")]
    [HasPermission(HrPermissions.EssOvertimeCreate)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelMyOvertimeRequest([FromRoute] OvertimeRequestId overtimeRequestId,
        CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var email = HttpContext.GetClaimValue("email");
        var res = await sender.Send(new CancelMyOvertimeRequestCommand(userId, email, overtimeRequestId), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}

public sealed record CancelMyLeaveRequestBody(string Reason);
