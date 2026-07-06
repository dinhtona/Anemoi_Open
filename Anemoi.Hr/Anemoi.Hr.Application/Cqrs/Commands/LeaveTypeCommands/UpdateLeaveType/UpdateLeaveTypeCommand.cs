using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveTypeCommands.UpdateLeaveType;

public sealed record UpdateLeaveTypeCommand(
    LeaveTypeId Id,
    string Code,
    string Name,
    bool IsPaid,
    bool RequiresApproval,
    decimal AnnualEntitlement,
    bool CarryForwardAllowed,
    decimal MaxCarryForwardDays) : ICommandResult<LeaveTypeResponse>;
