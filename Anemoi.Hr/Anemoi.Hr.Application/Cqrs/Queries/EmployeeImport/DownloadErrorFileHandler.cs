using System.Text.Json;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.BulkImport.Abstractions;
using Anemoi.BuildingBlock.Application.BulkImport.Models;
using Anemoi.BuildingBlock.Application.BulkImport.Services;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.BulkImport.EmployeeImport;
using Anemoi.Hr.Application.BulkImport.EmployeeImport.Validation;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.BulkImport;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeImport;

public sealed class DownloadErrorFileHandler(
    ISqlRepository<BulkImportJob> jobRepository,
    SyntaxValidator syntaxValidator,
    BusinessValidator businessValidator,
    BulkExcelTemplateService templateService,
    EmployeeImportTemplateProvider templateProvider)
    : IQueryHandler<DownloadErrorFileQuery, OneOf<FileResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<FileResponse, ErrorDetailResponse>> Handle(
        DownloadErrorFileQuery request, CancellationToken ct)
    {
        var job = await jobRepository.GetFirstByConditionAsync(
            j => j.Id == request.JobId, null, ct);
        if (job is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ImportJobNotFound);

        var preview = JsonSerializer.Deserialize<ImportPreviewData>(job.PreviewDataJson);
        if (preview is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ImportFileEmpty);

        var rows = preview.Rows.Select(r => new EmployeeImportRowDto { RawData = r.Data }).ToList();
        var allErrors = new List<BulkImportValidationError>();
        for (var i = 0; i < rows.Count; i++)
        {
            allErrors.AddRange(await syntaxValidator.ValidateAsync(rows[i], i, ct));
            allErrors.AddRange(await businessValidator.ValidateAsync(rows[i], i, ct));
        }

        var rowResults = preview.Rows.Select((r, i) =>
        {
            var errs = allErrors.Where(e => e.RowIndex == i).ToList();
            return new BulkImportRowResult(i,
                errs.Any(e => e.Severity == "Error") ? BulkImportRowStatus.Failed : BulkImportRowStatus.Success,
                null, errs.Count > 0 ? errs : null);
        }).ToList();

        var bbRows = preview.Rows.Select(r => new Anemoi.BuildingBlock.Application.BulkImport.Models.ImportPreviewRow(
            r.RowIndex, r.Data, r.ValidationStatus,
            r.Errors?.Select(e => new BulkImportValidationError(
                e.RowIndex, e.Column, e.Value, e.ErrorMessage, e.Severity)).ToList())).ToList();

        var errorBytes = templateService.GenerateErrorFile(
            bbRows, rowResults, templateProvider.Columns);

        var errorFileName = $"errors_{Path.GetFileNameWithoutExtension(job.OriginalFileName)}.xlsx";
        return new FileResponse(errorBytes, BulkImportConstants.ExcelContentType, errorFileName);
    }
}
