using System.Text.Json.Serialization;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.SuspendEmployee;

public sealed record SuspendEmployeeCommand(
    EmployeeId EmployeeId,
    [property: JsonIgnore] string CreatedBy = null) : ICommandVoid;
