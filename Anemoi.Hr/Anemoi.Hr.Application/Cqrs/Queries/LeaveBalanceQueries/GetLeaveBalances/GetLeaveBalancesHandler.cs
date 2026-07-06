using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Leaves;

namespace Anemoi.Hr.Application.Cqrs.Queries.LeaveBalanceQueries.GetLeaveBalances;

public sealed class GetLeaveBalancesHandler(ISqlRepository<LeaveBalance> repository, LeaveMapper mapper)
    : IQueryHandler<GetLeaveBalancesQuery, PaginationResponse<LeaveBalanceResponse>>
{
    public async Task<PaginationResponse<LeaveBalanceResponse>> Handle(GetLeaveBalancesQuery request,
        CancellationToken cancellationToken)
    {
        var page = await repository.GetManyByConditionWithPaginationAsync(
            x => (request.EmployeeId == null || x.EmployeeId == request.EmployeeId) &&
                (request.Year == null || x.Year == request.Year),
            q => q.OrderByWithDynamic(request.SortedFieldName, x => x.Year,
                    request.SortedDirection ?? SortedDirection.Descending)
                .Offset(request.GetSkip())
                .Limit(request.GetTake()),
            cancellationToken);

        return new PaginationResponse<LeaveBalanceResponse>(
            page.Items.Select(mapper.ToLeaveBalanceResponse).ToList(),
            page.TotalRecord);
    }
}
