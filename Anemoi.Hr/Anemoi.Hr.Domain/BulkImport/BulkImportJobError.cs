namespace Anemoi.Hr.Domain.BulkImport;

public sealed record BulkImportJobError(
    int RowIndex,
    string Column,
    string Value,
    string ErrorMessage,
    string Severity);
