using Anemoi.BuildingBlock.Application.BulkImport.Models;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeImport;

public sealed record EmployeeImportPreviewResponse(
    BulkImportJobId JobId,
    int TotalRows,
    int ValidRows,
    int WarningCount,
    int ErrorCount,
    IReadOnlyCollection<BulkImportValidationError> Errors,
    IReadOnlyCollection<BulkImportRowResult> RowResults);

public sealed record EmployeeImportResultResponse(
    BulkImportJobId JobId,
    int TotalRows,
    int ImportedRows,
    int FailedRows,
    string Status,
    byte[]? ErrorFileBytes,
    string? ErrorFileName);
