using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.SearchJobPostings;

public sealed record SearchJobPostingsQuery(
    string SearchTerm,
    string Status,
    JobRequisitionId JobRequisitionId) : GetManyQuery, IQueryPaged<JobPostingResponse>;
