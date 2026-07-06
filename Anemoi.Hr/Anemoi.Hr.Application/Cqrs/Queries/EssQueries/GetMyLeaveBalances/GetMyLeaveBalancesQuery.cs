using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyLeaveBalances;

public sealed record GetMyLeaveBalancesQuery(string UserId, string Email) : IQueryOne<IReadOnlyCollection<EssLeaveBalanceResponse>>;
