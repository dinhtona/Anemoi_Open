using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeImport;

public sealed record EmployeeImportHistoryResponse(
    BulkImportJobId Id,
    string OriginalFileName,
    int TotalRows,
    int ImportedRows,
    int FailedRows,
    string Status,
    string ImportedBy,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    long ExecutionDurationMs);

public sealed record EmployeeImportJobDetailResponse(
    BulkImportJobId Id,
    string EntityType,
    string OriginalFileName,
    int TotalRows,
    int ImportedRows,
    int FailedRows,
    string Status,
    string ImportedBy,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    long ExecutionDurationMs,
    string? ErrorDetails);
