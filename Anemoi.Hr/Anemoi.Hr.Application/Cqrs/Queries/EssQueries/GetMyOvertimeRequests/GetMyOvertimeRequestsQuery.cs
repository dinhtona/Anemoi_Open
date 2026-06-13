using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyOvertimeRequests;

public sealed record GetMyOvertimeRequestsQuery(string UserId, string Email) : IQueryOne<IReadOnlyCollection<EssOvertimeRequestResponse>>;
