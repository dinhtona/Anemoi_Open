using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.ApproveLeaveRequest;

[Obsolete("Use ApproveWorkflowStepCommand instead")]
public sealed record ApproveLeaveRequestCommand(LeaveRequestId? Id, EmployeeId ApproverEmployeeId, string Comment)
    : ICommandVoid;
