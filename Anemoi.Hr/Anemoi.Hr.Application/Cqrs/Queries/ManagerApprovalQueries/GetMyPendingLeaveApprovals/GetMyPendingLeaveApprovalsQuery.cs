using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.ManagerApprovalQueries.GetMyPendingLeaveApprovals;

public sealed record GetMyPendingLeaveApprovalsQuery(string? UserId)
    : IQueryOne<IReadOnlyCollection<ManagerLeavePendingApprovalResponse>>;
