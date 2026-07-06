using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.AttendanceCommands.CreateAttendancePeriod;
using Anemoi.Hr.Application.Cqrs.Commands.AttendanceCommands.CreateAttendanceRecord;
using Anemoi.Hr.Application.Cqrs.Commands.AttendanceCommands.LockAttendancePeriod;
using Anemoi.Hr.Application.Cqrs.Commands.AttendanceCommands.UpdateAttendanceRecord;
using Anemoi.Hr.Application.Cqrs.Queries.AttendanceQueries.GetAttendancePeriods;
using Anemoi.Hr.Application.Cqrs.Queries.AttendanceQueries.GetAttendanceRecordDetail;
using Anemoi.Hr.Application.Cqrs.Queries.AttendanceQueries.GetAttendanceRecords;
using Anemoi.Hr.Application.Cqrs.Queries.AttendanceQueries.GetEmployeeAttendanceSummary;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Api.Controllers.Attendance;

[ApiController]
[Route("api/hr/attendance/Attendance/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class AttendanceController(ISender sender) : ControllerBase
{
    [HttpPost]
    [HasPermission(HrPermissions.AttendanceCreate)]
    [ProducesResponseType(typeof(AttendancePeriodResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateAttendancePeriod(
        [FromBody] CreateAttendancePeriodCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CreatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.AttendanceCreate)]
    [ProducesResponseType(typeof(AttendanceRecordResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateAttendanceRecord(
        [FromBody] CreateAttendanceRecordCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CreatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPut]
    [HasPermission(HrPermissions.AttendanceUpdate)]
    [ProducesResponseType(typeof(AttendanceRecordResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAttendanceRecord(
        [FromBody] UpdateAttendanceRecordCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { UpdatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.AttendanceLock)]
    [ProducesResponseType(typeof(AttendancePeriodResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> LockAttendancePeriod(
        [FromBody] LockAttendancePeriodCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { UpdatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.AttendanceView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<AttendancePeriodResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAttendancePeriods(CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetAttendancePeriodsQuery(), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.AttendanceView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<AttendanceRecordResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAttendanceRecords(
        [FromQuery] AttendancePeriodId? attendancePeriodId,
        [FromQuery] EmployeeId? employeeId,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetAttendanceRecordsQuery(attendancePeriodId, employeeId), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.AttendanceView)]
    [ProducesResponseType(typeof(AttendanceRecordDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAttendanceRecordDetail(
        [FromRoute] AttendanceRecordId id,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetAttendanceRecordDetailQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.AttendanceView)]
    [ProducesResponseType(typeof(AttendanceSummaryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeAttendanceSummary(
        [FromQuery] AttendancePeriodId attendancePeriodId,
        [FromQuery] EmployeeId employeeId,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetEmployeeAttendanceSummaryQuery(attendancePeriodId, employeeId), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}
