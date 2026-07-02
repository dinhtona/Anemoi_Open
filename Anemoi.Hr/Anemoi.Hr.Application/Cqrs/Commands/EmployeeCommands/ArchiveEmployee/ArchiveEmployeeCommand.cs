using System.Text.Json.Serialization;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.ArchiveEmployee;

public sealed record ArchiveEmployeeCommand(
    EmployeeId EmployeeId,
    [property: JsonIgnore] string CreatedBy = null) : ICommandVoid;
