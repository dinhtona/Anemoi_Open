using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.CancelLeaveRequest;

public sealed record CancelLeaveRequestCommand(LeaveRequestId Id, EmployeeId CancelledByEmployeeId, string Reason)
    : ICommandVoid;
