using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentRequests;

public sealed record GetRecruitmentRequestsQuery(
    string? Status,
    string? DepartmentId,
    string? PositionId,
    string? SearchTerm,
    int Page = 1,
    int PageSize = 20) : IQueryPaged<RecruitmentRequestResponse>;
