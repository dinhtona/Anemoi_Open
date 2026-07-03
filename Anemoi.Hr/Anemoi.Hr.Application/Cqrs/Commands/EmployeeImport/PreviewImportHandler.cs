using System.Text.Json;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.BulkImport.Models;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.BulkImport;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeImport;

public sealed class PreviewImportHandler(
    ISqlRepository<BulkImportJob> jobRepository)
    : ICommandHandler<PreviewImportCommand, OneOf<EmployeeImportPreviewResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<EmployeeImportPreviewResponse, ErrorDetailResponse>> Handle(
        PreviewImportCommand request, CancellationToken ct)
    {
        var job = await jobRepository.GetFirstByConditionAsync(
            j => j.Id == request.JobId, null, ct);
        if (job is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ImportJobNotFound);

        var preview = JsonSerializer.Deserialize<ImportPreviewData>(job.PreviewDataJson);
        if (preview is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ImportFileEmpty);

        return new EmployeeImportPreviewResponse(
            job.Id,
            preview.TotalRows,
            preview.ValidRows,
            preview.WarningCount,
            preview.ErrorCount,
            preview.AllErrors.Select(e => new BulkImportValidationError(
                e.RowIndex, e.Column, e.Value, e.ErrorMessage, e.Severity)).ToList(),
            preview.Rows.Select(r =>
            {
                var errors = r.Errors?.Select(e => new BulkImportValidationError(
                    e.RowIndex, e.Column, e.Value, e.ErrorMessage, e.Severity)).ToList();
                return new BulkImportRowResult(r.RowIndex,
                    r.ValidationStatus == "Error" ? BulkImportRowStatus.Failed : BulkImportRowStatus.Success,
                    null, errors);
            }).ToList());
    }
}
