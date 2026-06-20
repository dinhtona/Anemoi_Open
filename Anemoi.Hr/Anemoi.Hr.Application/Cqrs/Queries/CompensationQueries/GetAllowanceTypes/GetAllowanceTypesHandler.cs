using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Compensation;

namespace Anemoi.Hr.Application.Cqrs.Queries.CompensationQueries.GetAllowanceTypes;

public sealed class GetAllowanceTypesHandler(
    ISqlRepository<AllowanceType> repository,
    CompensationMapper mapper)
    : IQueryHandler<GetAllowanceTypesQuery, PaginationResponse<AllowanceTypeResponse>>
{
    public async Task<PaginationResponse<AllowanceTypeResponse>> Handle(
        GetAllowanceTypesQuery request, CancellationToken cancellationToken)
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

        return new PaginationResponse<AllowanceTypeResponse>(
            page.Items.Select(mapper.ToAllowanceTypeResponse).ToList(),
            page.TotalRecord);
    }
}
