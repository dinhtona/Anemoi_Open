#nullable enable
using System.Collections.Generic;

namespace Anemoi.BuildingBlock.Application.BulkImport.Models;

public sealed record BulkImportResult(
    int TotalRows,
    int ImportedRows,
    int FailedRows,
    IReadOnlyCollection<BulkImportRowResult> RowResults);
