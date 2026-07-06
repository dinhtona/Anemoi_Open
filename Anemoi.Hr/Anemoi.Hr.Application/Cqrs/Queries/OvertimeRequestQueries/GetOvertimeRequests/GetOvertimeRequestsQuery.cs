#nullable enable

using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.OvertimeRequestQueries.GetOvertimeRequests;

public sealed record GetOvertimeRequestsQuery(
    EmployeeId? EmployeeId = null,
    string? Status = null,
    DateOnly? OvertimeDate = null,
    int Page = 1,
    int PageSize = 50,
    string? SortBy = null,
    string? SortDirection = "asc") : IQuery<PaginationResponse<OvertimeRequestResponse>>;
