using System.Text.Json.Serialization;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeNoteCommands.ArchiveNote;

public sealed record ArchiveEmployeeNoteCommand(
    EmployeeNoteId Id,
    [property: JsonIgnore] string ArchivedByUserId = null
) : ICommandVoid;
