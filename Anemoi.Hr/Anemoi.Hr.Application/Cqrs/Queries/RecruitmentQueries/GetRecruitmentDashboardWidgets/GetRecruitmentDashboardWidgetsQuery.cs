using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentDashboardWidgets;

public sealed record GetRecruitmentDashboardWidgetsQuery() : IQueryOne<RecruitmentDashboardWidgetsResponse>;
