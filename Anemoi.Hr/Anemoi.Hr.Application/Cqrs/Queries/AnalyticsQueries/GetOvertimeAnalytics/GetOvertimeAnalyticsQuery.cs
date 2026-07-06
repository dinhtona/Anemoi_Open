using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetOvertimeAnalytics;

public sealed record GetOvertimeAnalyticsQuery : IQuery<OvertimeAnalyticsResponse>;
