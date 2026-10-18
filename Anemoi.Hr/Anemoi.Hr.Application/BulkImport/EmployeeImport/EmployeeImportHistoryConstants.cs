namespace Anemoi.Hr.Application.BulkImport.EmployeeImport;

public static class EmployeeImportHistoryConstants
{
    public const string EntityType = "Employee";
    public const string EventType = "BulkImported";

    /// <summary>
    /// Fallback display title stored on the history record. The user-facing title is
    /// resolved at read time via the "Timeline.BulkImported.Title" localization key.
    /// </summary>
    public const string TitleFallback = "Bulk import";
}
