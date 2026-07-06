#nullable enable
using System.Collections.Generic;

namespace Anemoi.BuildingBlock.Application.BulkImport.Models;

public sealed record BulkImportPreview(
    int TotalRows,
    int ValidRows,
    int WarningCount,
    int ErrorCount,
    IReadOnlyCollection<BulkImportValidationError> AllErrors,
    IReadOnlyCollection<BulkImportRowResult> RowResults);
