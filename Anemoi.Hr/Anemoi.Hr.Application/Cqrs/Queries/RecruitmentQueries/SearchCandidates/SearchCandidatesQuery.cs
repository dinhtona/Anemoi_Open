using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.SearchCandidates;

public sealed record SearchCandidatesQuery(
    string? SearchTerm,
    string? Status,
    string? Source) : GetManyQuery, IQueryPaged<CandidateResponse>;
