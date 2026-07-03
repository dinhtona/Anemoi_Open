using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.BulkImport;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeImport;

public sealed class GetImportJobDetailHandler(
    ISqlRepository<BulkImportJob> jobRepository)
    : IQueryHandler<GetImportJobDetailQuery, OneOf<EmployeeImportJobDetailResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<EmployeeImportJobDetailResponse, ErrorDetailResponse>> Handle(
        GetImportJobDetailQuery request, CancellationToken ct)
    {
        var job = await jobRepository.GetFirstByConditionAsync(
            j => j.Id == request.JobId, null, ct);
        if (job is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ImportJobNotFound);

        return new EmployeeImportJobDetailResponse(
            job.Id,
            job.EntityType,
            job.OriginalFileName,
            job.TotalRows,
            job.ImportedRows,
            job.FailedRows,
            job.StatusCode,
            job.ActorName ?? "",
            job.CreatedAt,
            job.CompletedAt,
            job.ExecutionDurationMs,
            job.ErrorDetails);
    }
}
