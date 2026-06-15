using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.LeaveTransactionQueries.GetLeaveTransactions;

public sealed record GetLeaveTransactionsQuery(
    EmployeeId? EmployeeId,
    LeaveBalanceId? LeaveBalanceId,
    LeaveRequestId? LeaveRequestId,
    string? TransactionTypeCode) : GetManyQuery, IQueryPaged<LeaveTransactionResponse>;
