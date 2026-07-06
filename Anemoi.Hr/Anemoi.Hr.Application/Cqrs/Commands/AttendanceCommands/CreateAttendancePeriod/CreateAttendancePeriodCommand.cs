using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using System;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.AttendanceCommands.CreateAttendancePeriod;

public sealed record CreateAttendancePeriodCommand(
    string PeriodCode,
    DateOnly StartDate,
    DateOnly EndDate,
    [property: JsonIgnore] string CreatedBy = null) : ICommandResult<AttendancePeriodResponse>;
