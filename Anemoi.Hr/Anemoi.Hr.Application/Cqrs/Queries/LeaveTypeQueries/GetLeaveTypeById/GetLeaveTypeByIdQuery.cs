using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.LeaveTypeQueries.GetLeaveTypeById;

public sealed record GetLeaveTypeByIdQuery(LeaveTypeId Id) : IQueryOne<LeaveTypeResponse>;
