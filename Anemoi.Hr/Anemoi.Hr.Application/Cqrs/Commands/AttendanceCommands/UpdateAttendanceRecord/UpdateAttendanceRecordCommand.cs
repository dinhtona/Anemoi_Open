using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.AttendanceCommands.UpdateAttendanceRecord;

public sealed record UpdateAttendanceRecordCommand(
    AttendanceRecordId Id,
    AttendancePeriodId AttendancePeriodId,
    EmployeeId EmployeeId,
    DateOnly WorkDate,
    TimeOnly? CheckInTime,
    TimeOnly? CheckOutTime,
    decimal WorkedHours,
    decimal WorkedDays,
    string StatusCode,
    LeaveRequestId LeaveRequestId,
    [property: JsonIgnore] string UpdatedBy = null) : ICommandResult<AttendanceRecordResponse>;
