namespace Anemoi.Hr.Domain.BulkImport;

public sealed record ImportPreviewData(
    string OriginalFileName,
    int TotalRows,
    int ValidRows,
    int WarningCount,
    int ErrorCount,
    List<ImportPreviewRow> Rows,
    List<BulkImportJobError> AllErrors);

public sealed record ImportPreviewRow(
    int RowIndex,
    Dictionary<string, string> Data,
    string ValidationStatus,
    List<BulkImportJobError>? Errors);
