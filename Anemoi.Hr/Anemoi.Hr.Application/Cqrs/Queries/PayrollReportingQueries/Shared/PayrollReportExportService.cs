#nullable enable

using System.Security.Cryptography;
using System.Text.Json;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Reporting;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.Shared;

public sealed class PayrollReportExportService(
    IReportExporter reportExporter,
    ICurrentUser currentUser,
    ISqlRepository<ReportExportAuditLog> auditLogRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<OneOf<ExportResult, ErrorDetailResponse>> ExportAsync<T>(
        IReadOnlyCollection<T> records,
        string reportType,
        object filters,
        CancellationToken cancellationToken)
        where T : class
    {
        var exportedBy = currentUser.UserId;
        if (string.IsNullOrWhiteSpace(exportedBy))
            return HrErrorResponses.Create(HrBusinessErrorCodes.ReportExportPermissionDenied);

        if (records.Count > ReportExportLimits.MaxRows)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ReportExportLimitExceeded);

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
            return HrErrorResponses.FromSaveResult(createResult.AsT1, HrBusinessErrorCodes.ReportExportAuditLogSaveFailed);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.ReportExportAuditLogSaveFailed);

        return result;
    }
}
