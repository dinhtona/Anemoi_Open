using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetPayrollAnalytics;

public sealed record GetPayrollAnalyticsQuery : IQuery<PayrollAnalyticsResponse>;
