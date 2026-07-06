using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyLeaveRequests;

public sealed record GetMyLeaveRequestsQuery(string UserId, string Email) : IQueryOne<IReadOnlyCollection<EssLeaveRequestResponse>>;
