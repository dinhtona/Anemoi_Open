using System.Text.Json.Serialization;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeDocumentCommands.UpdateDocument;

public sealed record UpdateEmployeeDocumentCommand(
    EmployeeDocumentId Id,
    string DocumentType,
    string DisplayName,
    string? ReferenceNumber,
    string? IssuedBy,
    DateOnly? IssuedDate,
    DateOnly? ExpiryDate,
    string? StorageKey,
    string? FileName,
    string? MimeType,
    long? FileSize,
    string? Notes,
    [property: JsonIgnore] string UpdatedBy = null
) : ICommandVoid;
