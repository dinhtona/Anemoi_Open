using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.LeaveTypeQueries.GetLeaveTypes;

public sealed record GetLeaveTypesQuery(string? SearchKey, bool? IsActive)
    : GetManyQuery, IQueryPaged<LeaveTypeResponse>;
