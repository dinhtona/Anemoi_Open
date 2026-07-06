using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.LeaveRequestQueries.GetLeaveRequests;

public sealed record GetLeaveRequestsQuery(
    EmployeeId? EmployeeId,
    string? StatusCode,
    DateOnly? FromDate,
    DateOnly? ToDate) : GetManyQuery, IQueryPaged<LeaveRequestResponse>;
