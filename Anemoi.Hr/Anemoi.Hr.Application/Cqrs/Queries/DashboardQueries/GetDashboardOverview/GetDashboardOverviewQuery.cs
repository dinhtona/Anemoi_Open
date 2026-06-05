using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.DashboardQueries.GetDashboardOverview;

public sealed record GetDashboardOverviewQuery : IQuery<DashboardOverviewResponse>;
