using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetAnalyticsDashboard;

public sealed record GetAnalyticsDashboardQuery : IQuery<AnalyticsDashboardResponse>;
