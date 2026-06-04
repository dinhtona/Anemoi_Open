using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.LeaveBalanceQueries.GetLeaveBalances;

public sealed record GetLeaveBalancesQuery(EmployeeId EmployeeId, int? Year) : GetManyQuery,
    IQueryPaged<LeaveBalanceResponse>;
