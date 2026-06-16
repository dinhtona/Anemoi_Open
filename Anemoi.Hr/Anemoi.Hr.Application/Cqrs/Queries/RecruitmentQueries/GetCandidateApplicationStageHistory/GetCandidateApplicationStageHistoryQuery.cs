using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetCandidateApplicationStageHistory;

public sealed record GetCandidateApplicationStageHistoryQuery(CandidateApplicationId Id)
    : IQueryOne<IReadOnlyCollection<CandidateApplicationStageHistoryResponse>>;
