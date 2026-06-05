using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Positions;

namespace Anemoi.Hr.Application.Cqrs.Queries.PositionQueries.GetPositions;

public sealed class GetPositionsHandler(ISqlRepository<Position> repository, EmployeeMapper mapper)
    : IQueryHandler<GetPositionsQuery, PaginationResponse<PositionResponse>>
{
    public async Task<PaginationResponse<PositionResponse>> Handle(GetPositionsQuery request,
        CancellationToken cancellationToken)
    {
        var page = await repository.GetManyByConditionWithPaginationAsync(
            x => (string.IsNullOrEmpty(request.SearchKey) || 
                  x.Code.Contains(request.SearchKey) ||
                  x.Name.Contains(request.SearchKey)) &&
                 (request.DepartmentId == null || x.DepartmentId == request.DepartmentId) &&
                 (request.IsActive == null || x.IsActive == request.IsActive),
            q => q.OrderByWithDynamic(request.SortedFieldName, x => x.CreatedAt,
                    request.SortedDirection ?? SortedDirection.Descending)
                  .Offset(request.GetSkip())
                  .Limit(request.GetTake()),
            cancellationToken);

        return new PaginationResponse<PositionResponse>(
            page.Items.Select(mapper.ToPositionResponse).ToList(),
            page.TotalRecord);
    }
}
