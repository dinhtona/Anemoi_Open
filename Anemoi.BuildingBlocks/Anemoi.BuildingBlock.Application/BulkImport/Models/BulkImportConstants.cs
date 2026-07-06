#nullable enable
namespace Anemoi.BuildingBlock.Application.BulkImport.Models;

public static class BulkImportConstants
{
    public const int MaxUploadRows = 10000;
    public const long MaxFileSizeBytes = 10 * 1024 * 1024;
    public const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public const string CsvContentType = "text/csv";
}
