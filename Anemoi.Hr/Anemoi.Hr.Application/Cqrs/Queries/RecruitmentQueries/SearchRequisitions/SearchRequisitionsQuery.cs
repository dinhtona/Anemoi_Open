using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.SearchRequisitions;

public sealed record SearchRequisitionsQuery(
    string SearchTerm,
    string Status,
    DepartmentId DepartmentId) : GetManyQuery, IQueryPaged<JobRequisitionResponse>;
