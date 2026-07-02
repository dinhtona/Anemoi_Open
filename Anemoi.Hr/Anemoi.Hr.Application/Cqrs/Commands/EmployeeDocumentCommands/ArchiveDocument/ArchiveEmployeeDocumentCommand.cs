using System.Text.Json.Serialization;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeDocumentCommands.ArchiveDocument;

public sealed record ArchiveEmployeeDocumentCommand(
    EmployeeDocumentId Id,
    [property: JsonIgnore] string ArchivedBy = null
) : ICommandVoid;
