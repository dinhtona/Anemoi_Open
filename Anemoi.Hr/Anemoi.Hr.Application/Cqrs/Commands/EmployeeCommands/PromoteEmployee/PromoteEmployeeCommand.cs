using System.Text.Json.Serialization;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.PromoteEmployee;

public sealed record PromoteEmployeeCommand(
    EmployeeId EmployeeId,
    PositionId NewPositionId,
    string NewGradeCode,
    DateOnly EffectiveDate,
    string ReasonCode,
    bool SensitivePermissionConfirmed,
    [property: JsonIgnore] string CreatedBy = null) : ICommandResult<PromoteEmployeeResponse>;
