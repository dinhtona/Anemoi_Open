using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyAttendanceSummary;

public sealed record GetMyAttendanceSummaryQuery(string UserId, string Email) : IQueryOne<IReadOnlyCollection<EssAttendanceSummaryResponse>>;
