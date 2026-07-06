using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Leaves;

namespace Anemoi.Hr.Application.Cqrs.Queries.LeaveTransactionQueries.GetLeaveTransactions;

public sealed class GetLeaveTransactionsHandler(ISqlRepository<LeaveTransaction> repository, LeaveMapper mapper)
    : IQueryHandler<GetLeaveTransactionsQuery, PaginationResponse<LeaveTransactionResponse>>
{
    public async Task<PaginationResponse<LeaveTransactionResponse>> Handle(GetLeaveTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var page = await repository.GetManyByConditionWithPaginationAsync(
            x => (request.EmployeeId == null || x.EmployeeId == request.EmployeeId) &&
                (request.LeaveBalanceId == null || x.LeaveBalanceId == request.LeaveBalanceId) &&
                (request.LeaveRequestId == null || x.LeaveRequestId == request.LeaveRequestId) &&
                (string.IsNullOrEmpty(request.TransactionTypeCode) ||
                    x.TransactionTypeCode == request.TransactionTypeCode),
            q => q.OrderByWithDynamic(request.SortedFieldName, x => x.CreatedAt,
                    request.SortedDirection ?? SortedDirection.Descending)
                .Offset(request.GetSkip())
                .Limit(request.GetTake()),
            cancellationToken);

        return new PaginationResponse<LeaveTransactionResponse>(
            page.Items.Select(mapper.ToLeaveTransactionResponse).ToList(),
            page.TotalRecord);
    }
}
