using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using System;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentOverview;

public sealed record GetRecruitmentOverviewQuery(
    DateOnly? FromDate,
    DateOnly? ToDate) : IQuery<RecruitmentOverviewResponse>;
