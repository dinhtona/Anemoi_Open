using Anemoi.BuildingBlock.Application.BulkImport.Models;
using HrBulkImportJobError = Anemoi.Hr.Domain.BulkImport.BulkImportJobError;

namespace Anemoi.Hr.Application.BulkImport.Models;

public sealed record ImportPreviewData(
    string OriginalFileName,
    int TotalRows,
    int ValidRows,
    int WarningCount,
    int ErrorCount,
    List<ImportPreviewRow> Rows,
    List<HrBulkImportJobError> AllErrors);
