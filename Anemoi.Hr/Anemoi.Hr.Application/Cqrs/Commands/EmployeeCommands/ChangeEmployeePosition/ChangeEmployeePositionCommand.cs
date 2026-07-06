using System.Text.Json.Serialization;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.ChangeEmployeePosition;

public sealed record ChangeEmployeePositionCommand(
    EmployeeId EmployeeId,
    PositionId NewPositionId,
    [property: JsonIgnore] string CreatedBy = null) : ICommandVoid;
