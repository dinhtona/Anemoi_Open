using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.MasterData;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.LeavePolicyQueries.GetLeavePolicies;

public sealed class GetLeavePoliciesHandler(
    ISqlRepository<LeavePolicy> repository,
    MasterDataMapper mapper)
    : IQueryHandler<GetLeavePoliciesQuery, PaginationResponse<LeavePolicyResponse>>
{
    public async Task<PaginationResponse<LeavePolicyResponse>> Handle(
        GetLeavePoliciesQuery request, CancellationToken cancellationToken)
    {
        var page = await repository.GetManyByConditionWithPaginationAsync(
            x => (string.IsNullOrEmpty(request.SearchKey) ||
                  x.Code.Contains(request.SearchKey) ||
                  x.Name.Contains(request.SearchKey)) &&
                 (request.IsActive == null || x.IsActive == request.IsActive),
            q => q.Include(x => x.LeaveType)
                  .OrderByWithDynamic(request.SortedFieldName, x => x.CreatedAt,
                      request.SortedDirection ?? SortedDirection.Descending)
                  .Offset(request.GetSkip())
                  .Limit(request.GetTake()),
            cancellationToken);

        return new PaginationResponse<LeavePolicyResponse>(
            page.Items.Select(mapper.ToLeavePolicyResponse).ToList(),
            page.TotalRecord);
    }
}
