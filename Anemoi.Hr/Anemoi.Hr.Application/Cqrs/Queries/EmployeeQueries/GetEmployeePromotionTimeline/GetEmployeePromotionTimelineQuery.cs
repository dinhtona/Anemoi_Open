using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.GetEmployeePromotionTimeline;

public sealed record GetEmployeePromotionTimelineQuery(
    EmployeeId EmployeeId) : IQuery<IReadOnlyCollection<EmployeePromotionTimelineResponse>>;
