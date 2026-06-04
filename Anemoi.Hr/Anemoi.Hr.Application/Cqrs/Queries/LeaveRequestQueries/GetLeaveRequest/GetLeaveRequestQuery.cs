using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.LeaveRequestQueries.GetLeaveRequest;

public sealed record GetLeaveRequestQuery(LeaveRequestId Id) : IQueryOne<LeaveRequestResponse>;
