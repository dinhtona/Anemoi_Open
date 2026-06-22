using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.LifecycleQueries.GetEmployeeTimeline;

public sealed record GetEmployeeTimelineQuery(
    EmployeeId? EmployeeId,
    string? EventType,
    string? EntityType,
    DateTime? DateFrom,
    DateTime? DateTo) : GetManyQuery, IQueryPaged<EmployeeHistoryDto>;
