using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;

namespace Anemoi.Hr.Application.Cqrs.Queries.LifecycleQueries.GetLifecycleSummary;

public sealed record GetLifecycleSummaryQuery : IQueryOne<DashboardLifecycleSummaryDto>;
