using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.RejectLeaveRequest;

public sealed record RejectLeaveRequestCommand(LeaveRequestId Id, EmployeeId ApproverEmployeeId, string Comment)
    : ICommandVoid;
