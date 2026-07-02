using System.Text.Json.Serialization;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeDocumentCommands.CreateDocument;

public sealed record CreateEmployeeDocumentCommand(
    EmployeeId EmployeeId,
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
    [property: JsonIgnore] string CreatedBy = null
) : ICommandVoid;
