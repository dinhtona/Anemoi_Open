using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Reporting;

/// <summary>
/// Records every HR report export event for audit traceability.
///
/// Retention Policy:
/// - Export audit records MUST be retained for a minimum of 5 years.
/// - Future Tax and Insurance modules may rely on this export traceability.
/// - These records must NOT be deleted casually.
/// - Any future cleanup job MUST respect the retention policy defined above.
/// </summary>
public sealed class ReportExportAuditLog : Entity<ReportExportAuditLogId>
{
    public string ModuleCode { get; private set; } = default!;
    public string ReportType { get; private set; } = default!;
    public string ExportedBy { get; private set; } = default!;
    public DateTime ExportedAt { get; private set; }
    public string Format { get; private set; } = default!;
    public int TotalRecords { get; private set; }
    public string FiltersJson { get; private set; } = "{}";
    public string FileHash { get; private set; } = default!;
    public string FileName { get; private set; } = default!;

    private ReportExportAuditLog() { }

    public static ReportExportAuditLog Export(
        ReportExportAuditLogId id,
        string moduleCode,
        string reportType,
        string exportedBy,
        string format,
        int totalRecords,
        string filtersJson,
        string fileHash,
        string fileName)
    {
        if (string.IsNullOrWhiteSpace(moduleCode))
            throw new ArgumentException("Module code must not be null or whitespace.", nameof(moduleCode));

        if (string.IsNullOrWhiteSpace(reportType))
            throw new ArgumentException("Report type must not be null or whitespace.", nameof(reportType));

        if (string.IsNullOrWhiteSpace(exportedBy))
            throw new ArgumentException("Exported by user identifier must not be null or whitespace.", nameof(exportedBy));

        if (string.IsNullOrWhiteSpace(format))
            throw new ArgumentException("Format must not be null or whitespace.", nameof(format));

        if (totalRecords < 0)
            throw new ArgumentOutOfRangeException(nameof(totalRecords), "Total records must be greater than or equal to 0.");

        if (string.IsNullOrWhiteSpace(filtersJson))
            throw new ArgumentException("Filters JSON must not be null or whitespace.", nameof(filtersJson));

        if (string.IsNullOrWhiteSpace(fileHash))
            throw new ArgumentException("File hash must not be null or whitespace.", nameof(fileHash));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name must not be null or whitespace.", nameof(fileName));

        var auditLog = new ReportExportAuditLog
        {
            Id = id,
            ModuleCode = moduleCode,
            ReportType = reportType,
            ExportedBy = exportedBy,
            ExportedAt = DateTime.UtcNow,
            Format = format,
            TotalRecords = totalRecords,
            FiltersJson = filtersJson,
            FileHash = fileHash,
            FileName = fileName
        };

        return auditLog;
    }
}
