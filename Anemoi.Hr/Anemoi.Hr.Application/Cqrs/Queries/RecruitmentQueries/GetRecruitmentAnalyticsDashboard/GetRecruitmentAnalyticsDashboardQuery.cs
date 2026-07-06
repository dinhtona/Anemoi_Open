using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using System;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentAnalyticsDashboard;

public sealed record GetRecruitmentAnalyticsDashboardQuery(
    DateOnly? FromDate,
    DateOnly? ToDate) : IQuery<RecruitmentAnalyticsDashboardResponse>;
