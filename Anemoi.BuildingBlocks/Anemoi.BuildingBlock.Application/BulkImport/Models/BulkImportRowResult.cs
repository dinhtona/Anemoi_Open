#nullable enable
using System.Collections.Generic;

namespace Anemoi.BuildingBlock.Application.BulkImport.Models;

public sealed record BulkImportRowResult(
    int RowIndex,
    BulkImportRowStatus Status,
    string? EntityId,
    IReadOnlyCollection<BulkImportValidationError>? Errors);

public enum BulkImportRowStatus
{
    Success,
    Skipped,
    Failed
}
