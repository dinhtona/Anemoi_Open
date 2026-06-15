#nullable enable

using System.Security.Cryptography;
using System.Text.Json;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Reporting;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.Shared;

public sealed class PayrollReportExportService(
    IReportExporter reportExporter,
    ICurrentUser currentUser,
    ISqlRepository<ReportExportAuditLog> auditLogRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<ExportResult> ExportAsync<T>(
        IReadOnlyCollection<T> records,
        string reportType,
        object filters,
        CancellationToken cancellationToken)
        where T : class
    {
        var exportedBy = currentUser.UserId;
        if (string.IsNullOrWhiteSpace(exportedBy))
            throw new InvalidOperationException(HrBusinessErrorCodes.ReportExportPermissionDenied);

        if (records.Count > ReportExportLimits.MaxRows)
            throw new InvalidOperationException(HrBusinessErrorCodes.ReportExportLimitExceeded);

        var fileName = $"{reportType.Replace('.', '-')}-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        var result = reportExporter.ExportCsv(records, fileName);
        var fileHash = Convert.ToHexString(SHA256.HashData(result.Content));
        var filtersJson = JsonSerializer.Serialize(filters);

        var auditLog = ReportExportAuditLog.Export(
            new ReportExportAuditLogId(IdGenerator.NextGuid()),
            ModuleCodes.Payroll,
            reportType,
            exportedBy,
            "csv",
            records.Count,
            filtersJson,
            fileHash,
            result.FileName);

        var createResult = await auditLogRepository.CreateOneAsync(auditLog, cancellationToken);
        if (createResult.IsT1)
            throw new InvalidOperationException(HrBusinessErrorCodes.ReportExportAuditLogSaveFailed, createResult.AsT1);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            throw new InvalidOperationException(HrBusinessErrorCodes.ReportExportAuditLogSaveFailed, saveResult.AsT1);

        return result;
    }
}
