using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Leaves;

namespace Anemoi.Hr.Application.Cqrs.Queries.LeaveTypeQueries.GetLeaveTypes;

public sealed class GetLeaveTypesHandler(
    ISqlRepository<LeaveType> repository,
    MasterDataMapper mapper)
    : IQueryHandler<GetLeaveTypesQuery, PaginationResponse<LeaveTypeResponse>>
{
    public async Task<PaginationResponse<LeaveTypeResponse>> Handle(
        GetLeaveTypesQuery request, CancellationToken cancellationToken)
    {
        var page = await repository.GetManyByConditionWithPaginationAsync(
            x => (string.IsNullOrEmpty(request.SearchKey) ||
                  x.Code.Contains(request.SearchKey) ||
                  x.Name.Contains(request.SearchKey)) &&
                 (request.IsActive == null || x.IsActive == request.IsActive),
            q => q.OrderByWithDynamic(request.SortedFieldName, x => x.CreatedAt,
                    request.SortedDirection ?? SortedDirection.Descending)
                  .Offset(request.GetSkip())
                  .Limit(request.GetTake()),
            cancellationToken);

        return new PaginationResponse<LeaveTypeResponse>(
            page.Items.Select(mapper.ToLeaveTypeResponse).ToList(),
            page.TotalRecord);
    }
}
