#nullable enable
using System.Collections.Generic;
using Anemoi.BuildingBlock.Application.BulkImport.Models;

namespace Anemoi.BuildingBlock.Application.BulkImport.Abstractions;

public interface IBulkImportErrorFileGenerator
{
    byte[] GenerateErrorFile(
        IReadOnlyCollection<ImportPreviewRow> previewRows,
        IReadOnlyCollection<BulkImportRowResult> rowResults,
        IReadOnlyCollection<ImportColumnDefinition> columns);
}
