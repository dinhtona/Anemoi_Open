using System.Text.Json.Serialization;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeNoteCommands.CreateNote;

public sealed record CreateEmployeeNoteCommand(
    EmployeeId EmployeeId,
    string Content,
    string? NoteCategory,
    [property: JsonIgnore] string CreatedByUserId = null
) : ICommandVoid;
