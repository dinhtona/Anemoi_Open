#nullable enable
namespace Anemoi.BuildingBlock.Application.BulkImport.Models;

public sealed record BulkImportValidationError(
    int RowIndex,
    string Column,
    string Value,
    string ErrorMessage,
    string Severity);
