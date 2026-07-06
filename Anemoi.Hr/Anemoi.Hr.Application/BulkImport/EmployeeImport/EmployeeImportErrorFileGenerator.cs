using Anemoi.BuildingBlock.Application.BulkImport.Abstractions;
using Anemoi.BuildingBlock.Application.BulkImport.Models;
using Anemoi.BuildingBlock.Application.BulkImport.Services;

namespace Anemoi.Hr.Application.BulkImport.EmployeeImport;

public sealed class EmployeeImportErrorFileGenerator(
    BulkExcelTemplateService templateService,
    EmployeeImportTemplateProvider templateProvider)
    : IBulkImportErrorFileGenerator
{
    public byte[] GenerateErrorFile(
        IReadOnlyCollection<ImportPreviewRow> previewRows,
        IReadOnlyCollection<BulkImportRowResult> rowResults,
        IReadOnlyCollection<ImportColumnDefinition> columns)
    {
        return templateService.GenerateErrorFile(previewRows, rowResults, columns);
    }
}
