using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Overtime;

namespace Anemoi.Hr.Application.Cqrs.Queries.OvertimeRuleQueries.GetOvertimeRules;

public sealed class GetOvertimeRulesHandler(
    ISqlRepository<OvertimeRule> repository,
    MasterDataMapper mapper)
    : IQueryHandler<GetOvertimeRulesQuery, PaginationResponse<OvertimeRuleResponse>>
{
    public async Task<PaginationResponse<OvertimeRuleResponse>> Handle(
        GetOvertimeRulesQuery request, CancellationToken cancellationToken)
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

        return new PaginationResponse<OvertimeRuleResponse>(
            page.Items.Select(mapper.ToOvertimeRuleResponse).ToList(),
            page.TotalRecord);
    }
}
