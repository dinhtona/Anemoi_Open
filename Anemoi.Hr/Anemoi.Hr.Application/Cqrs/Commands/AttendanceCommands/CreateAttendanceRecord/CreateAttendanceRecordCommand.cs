using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.AttendanceCommands.CreateAttendanceRecord;

public sealed record CreateAttendanceRecordCommand(
    AttendancePeriodId AttendancePeriodId,
    EmployeeId EmployeeId,
    DateOnly WorkDate,
    TimeOnly? CheckInTime,
    TimeOnly? CheckOutTime,
    decimal WorkedHours,
    decimal WorkedDays,
    string StatusCode,
    LeaveRequestId LeaveRequestId,
    [property: JsonIgnore] string CreatedBy = null) : ICommandResult<AttendanceRecordResponse>;
