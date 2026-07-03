using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Domain.BulkImport;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeImport;

public sealed class GetImportHistoryHandler(
    ISqlRepository<BulkImportJob> jobRepository)
    : IQueryHandler<GetImportHistoryQuery, PaginationResponse<EmployeeImportHistoryResponse>>
{
    public async Task<PaginationResponse<EmployeeImportHistoryResponse>> Handle(
        GetImportHistoryQuery request, CancellationToken ct)
    {
        var page = await jobRepository.GetManyByConditionWithPaginationAsync(
            _ => true,
            q => q.OrderByDescending(j => j.CreatedAt)
                  .Offset(request.GetSkip())
                  .Limit(request.GetTake()),
            ct);

        return new PaginationResponse<EmployeeImportHistoryResponse>(
            page.Items.Select(j => new EmployeeImportHistoryResponse(
                j.Id,
                j.OriginalFileName,
                j.TotalRows,
                j.ImportedRows,
                j.FailedRows,
                j.StatusCode,
                j.ActorName ?? "",
                j.CreatedAt,
                j.CompletedAt,
                j.ExecutionDurationMs)).ToList(),
            page.TotalRecord);
    }
}
