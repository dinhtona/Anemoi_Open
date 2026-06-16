using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.SearchHiringDecisions;

public sealed record SearchHiringDecisionsQuery(
    string SearchTerm,
    string Decision) : GetManyQuery, IQueryPaged<HiringDecisionResponse>;
