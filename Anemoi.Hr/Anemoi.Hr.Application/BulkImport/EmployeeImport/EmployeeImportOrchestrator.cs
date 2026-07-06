using System.Text.Json;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.BulkImport.Models;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.BulkImport.Abstractions;
using Anemoi.Hr.Application.BulkImport.EmployeeImport.Validation;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.EmployeeImport;
using Anemoi.Hr.Domain.BulkImport;
using OneOf;

namespace Anemoi.Hr.Application.BulkImport.EmployeeImport;

public sealed class EmployeeImportOrchestrator(
    EmployeeImportHandler handler,
    SyntaxValidator syntaxValidator,
    BusinessValidator businessValidator,
    DuplicateValidator duplicateValidator,
    ISqlRepository<BulkImportJob> jobRepository,
    IUnitOfWork unitOfWork)
    : IImportOrchestrator
{
    public async Task<OneOf<EmployeeImportResultResponse, ErrorDetailResponse>> ExecuteImportAsync(
        BulkImportJob job,
        string actorUserId,
        string? actorName,
        CancellationToken ct)
    {
        var preview = JsonSerializer.Deserialize<ImportPreviewData>(job.PreviewDataJson);
        if (preview is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ImportFileEmpty);

        var rows = preview.Rows
            .Select(r => new EmployeeImportRowDto { RawData = r.Data })
            .ToList();

        var allErrors = new List<BulkImportValidationError>();
        for (var i = 0; i < rows.Count; i++)
        {
            allErrors.AddRange(await syntaxValidator.ValidateAsync(rows[i], i, ct));
            allErrors.AddRange(await businessValidator.ValidateAsync(rows[i], i, ct));
        }

        allErrors.AddRange(duplicateValidator.ValidateInFile(rows));
        allErrors.AddRange(await duplicateValidator.ValidateAgainstDatabaseAsync(rows, ct));

        var validRows = new List<EmployeeImportRowDto>();
        for (var i = 0; i < rows.Count; i++)
        {
            var hasError = allErrors.Any(e => e.RowIndex == i && e.Severity == "Error");
            if (!hasError) validRows.Add(rows[i]);
        }

        if (validRows.Count == 0)
        {
            job.StatusCode = BulkImportJobStatus.Failed;
            job.CompletedAt = DateTime.UtcNow;
            await unitOfWork.SaveChangesAsync(ct);
            return new EmployeeImportResultResponse(
                job.Id, rows.Count, 0, rows.Count, "Failed", null, null);
        }

        var handlerResult = await handler.HandleAsync(validRows, actorUserId, ct);

        if (handlerResult.IsT1)
        {
            return handlerResult.AsT1;
        }

        var rowResults = handlerResult.AsT0;
        var imported = rowResults.Count(r => r.Status == BulkImportRowStatus.Success);
        var failed = rowResults.Count(r => r.Status == BulkImportRowStatus.Failed);

        job.StatusCode = failed > 0 ? "CompletedWithErrors" : BulkImportJobStatus.Completed;
        job.ImportedRows = imported;
        job.FailedRows = failed;
        job.ActorUserId = actorUserId;
        job.ActorName = actorName;
        job.CompletedAt = DateTime.UtcNow;
        job.ExecutionDurationMs = (long)(DateTime.UtcNow - job.CreatedAt).TotalMilliseconds;

        var errorRows = rowResults
            .Where(r => r.Errors?.Count > 0)
            .SelectMany(r => r.Errors!)
            .Select(e => new BulkImportJobError(e.RowIndex, e.Column, e.Value, e.ErrorMessage, e.Severity))
            .ToList();

        if (errorRows.Count > 0)
            job.ErrorDetails = JsonSerializer.Serialize(errorRows);

        var saveResult = await unitOfWork.SaveChangesAsync(ct);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.SaveChangesFailed);

        return new EmployeeImportResultResponse(
            job.Id, rows.Count, imported, failed, job.StatusCode, null, null);
    }
}
