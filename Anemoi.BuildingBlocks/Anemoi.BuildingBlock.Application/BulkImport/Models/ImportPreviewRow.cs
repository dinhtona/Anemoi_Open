#nullable enable
using System.Collections.Generic;

namespace Anemoi.BuildingBlock.Application.BulkImport.Models;

public sealed record ImportPreviewRow(
    int RowIndex,
    Dictionary<string, string> Data,
    string ValidationStatus,
    List<BulkImportValidationError>? Errors);
