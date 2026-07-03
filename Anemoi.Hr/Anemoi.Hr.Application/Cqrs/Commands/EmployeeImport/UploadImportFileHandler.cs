using System.Text.Json;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.BulkImport.Abstractions;
using Anemoi.BuildingBlock.Application.BulkImport.Models;
using Anemoi.BuildingBlock.Application.BulkImport.Services;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.BulkImport.EmployeeImport;
using Anemoi.Hr.Application.BulkImport.EmployeeImport.Validation;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.BulkImport;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeImport;

public sealed class UploadImportFileHandler(
    BulkExcelParserService parser,
    SyntaxValidator syntaxValidator,
    BusinessValidator businessValidator,
    DuplicateValidator duplicateValidator,
    ReferenceValidator referenceValidator,
    ISqlRepository<BulkImportJob> jobRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UploadImportFileCommand, OneOf<EmployeeImportPreviewResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<EmployeeImportPreviewResponse, ErrorDetailResponse>> Handle(
        UploadImportFileCommand request, CancellationToken ct)
    {
        var ext = Path.GetExtension(request.FileName)?.ToLowerInvariant();
        if (ext != ".xlsx" && ext != ".csv")
            return HrErrorResponses.Create(HrBusinessErrorCodes.InvalidFileFormat);

        if (request.FileLength > BulkImportConstants.MaxFileSizeBytes)
            return HrErrorResponses.Create(HrBusinessErrorCodes.FileTooLarge);

        IReadOnlyCollection<Dictionary<string, string>> parsedData;
        try { parsedData = parser.Parse(request.FileBytes, request.FileName); }
        catch (Exception ex)
            { return HrErrorResponses.Create(HrBusinessErrorCodes.FileParseFailed); }

        if (parsedData.Count == 0)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ImportFileEmpty);
        if (parsedData.Count > BulkImportConstants.MaxUploadRows)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ImportTooManyRows);

        var rows = parsedData.Select(d => new EmployeeImportRowDto { RawData = d }).ToList();

        var allErrors = new List<BulkImportValidationError>();
        for (var i = 0; i < rows.Count; i++)
            allErrors.AddRange(await syntaxValidator.ValidateAsync(rows[i], i, ct));

        for (var i = 0; i < rows.Count; i++)
            allErrors.AddRange(await businessValidator.ValidateAsync(rows[i], i, ct));

        allErrors.AddRange(duplicateValidator.ValidateInFile(rows));

        allErrors.AddRange(await duplicateValidator.ValidateAgainstDatabaseAsync(rows, ct));

        var rowResults = new List<BulkImportRowResult>(rows.Count);
        var validRows = new List<EmployeeImportRowDto>();
        for (var i = 0; i < rows.Count; i++)
        {
            var rowErrors = allErrors.Where(e => e.RowIndex == i).ToList();
            var hasError = rowErrors.Any(e => e.Severity == "Error");
            rowResults.Add(new BulkImportRowResult(i,
                hasError ? BulkImportRowStatus.Failed : BulkImportRowStatus.Success,
                null, rowErrors.Count > 0 ? rowErrors : null));
            if (!hasError) validRows.Add(rows[i]);
        }

        var previewRows = rows.Select((r, i) =>
        {
            var rErrors = allErrors.Where(e => e.RowIndex == i).ToList();
            var hasErr = rErrors.Any(e => e.Severity == "Error");
            var hasWarn = rErrors.Any(e => e.Severity == "Warning");
            return new Domain.BulkImport.ImportPreviewRow(
                i, r.RawData,
                hasErr ? "Error" : hasWarn ? "Warning" : "Valid",
                rErrors.Select(e =>
                    new BulkImportJobError(e.RowIndex, e.Column, e.Value, e.ErrorMessage, e.Severity)).ToList());
        }).ToList();

        var preview = new ImportPreviewData(
            request.FileName,
            rows.Count,
            validRows.Count,
            allErrors.Count(e => e.Severity == "Warning"),
            allErrors.Count(e => e.Severity == "Error"),
            previewRows,
            allErrors.Select(e => new BulkImportJobError(
                e.RowIndex, e.Column, e.Value, e.ErrorMessage, e.Severity)).ToList());

        var jobId = new BulkImportJobId(IdGenerator.NextGuid());
        var job = new BulkImportJob
        {
            Id = jobId,
            EntityType = "Employee",
            OriginalFileName = request.FileName,
            StatusCode = BulkImportJobStatus.Pending,
            TotalRows = rows.Count,
            ImportedRows = 0,
            FailedRows = 0,
            PreviewDataJson = JsonSerializer.Serialize(preview),
            CreatedAt = DateTime.UtcNow
        };
        await jobRepository.CreateOneAsync(job, ct);

        var saveResult = await unitOfWork.SaveChangesAsync(ct);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.SaveChangesFailed);

        return new EmployeeImportPreviewResponse(
            jobId,
            preview.TotalRows,
            preview.ValidRows,
            preview.WarningCount,
            preview.ErrorCount,
            preview.AllErrors
                .Select(e => new BulkImportValidationError(
                    e.RowIndex, e.Column, e.Value, e.ErrorMessage, e.Severity))
                .ToList(),
            rowResults);
    }
}
