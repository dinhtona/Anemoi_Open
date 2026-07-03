using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.ManagerApprovalQueries.GetMyPendingOvertimeApprovals;

public sealed record GetMyPendingOvertimeApprovalsQuery(string? UserId, IReadOnlyCollection<string> RoleGroups)
    : IQueryOne<IReadOnlyCollection<ManagerOvertimePendingApprovalResponse>>;
