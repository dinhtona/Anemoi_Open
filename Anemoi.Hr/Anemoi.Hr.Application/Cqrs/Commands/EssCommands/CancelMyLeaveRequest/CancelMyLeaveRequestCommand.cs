using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.EssCommands.CancelMyLeaveRequest;

public sealed record CancelMyLeaveRequestCommand(
    string UserId,
    string Email,
    LeaveRequestId LeaveRequestId,
    string? Reason) : ICommandVoid;
