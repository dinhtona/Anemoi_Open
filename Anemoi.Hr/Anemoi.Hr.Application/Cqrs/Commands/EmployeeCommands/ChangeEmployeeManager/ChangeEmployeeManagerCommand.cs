using System.Text.Json.Serialization;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.ChangeEmployeeManager;

public sealed record ChangeEmployeeManagerCommand(
    EmployeeId EmployeeId,
    EmployeeId? NewManagerEmployeeId,
    [property: JsonIgnore] string CreatedBy = null) : ICommandVoid;
