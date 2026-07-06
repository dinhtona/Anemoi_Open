using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.SearchCandidateApplications;

public sealed record SearchCandidateApplicationsQuery(
    string? SearchTerm,
    string? CurrentStage,
    CandidateId? CandidateId,
    JobPostingId? JobPostingId) : GetManyQuery, IQueryPaged<CandidateApplicationResponse>;
