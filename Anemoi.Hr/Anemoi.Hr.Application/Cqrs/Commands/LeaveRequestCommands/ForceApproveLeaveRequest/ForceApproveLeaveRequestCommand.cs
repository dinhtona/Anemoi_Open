using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.ForceApproveLeaveRequest;

public sealed record ForceApproveLeaveRequestCommand(
    LeaveRequestId Id,
    EmployeeId ActorEmployeeId,
    string Reason,
    bool SensitivePermissionConfirmed) : ICommandVoid;
