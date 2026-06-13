using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyPayrollHistory;

public sealed record GetMyPayrollHistoryQuery(string UserId, string Email) : IQueryOne<IReadOnlyCollection<EssPayrollPeriodResponse>>;
