using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.AttendanceCommands.LockAttendancePeriod;

public sealed record LockAttendancePeriodCommand(
    AttendancePeriodId AttendancePeriodId,
    bool SensitivePermissionConfirmed,
    [property: JsonIgnore] string UpdatedBy = null) : ICommandResult<AttendancePeriodResponse>;
