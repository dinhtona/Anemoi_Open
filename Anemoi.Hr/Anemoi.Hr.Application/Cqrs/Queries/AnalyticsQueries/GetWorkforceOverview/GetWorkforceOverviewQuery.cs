using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetWorkforceOverview;

public sealed record GetWorkforceOverviewQuery : IQuery<WorkforceOverviewResponse>;
