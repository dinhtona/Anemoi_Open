using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.CompensationQueries.GetCompensationTimeline;

public sealed record GetCompensationTimelineQuery(
    EmployeeId EmployeeId) : IQuery<IReadOnlyCollection<CompensationTimelineItemResponse>>;
