using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveTypeCommands.CreateLeaveType;

public sealed record CreateLeaveTypeCommand(
    string Code,
    string Name,
    bool IsPaid,
    bool RequiresApproval,
    decimal AnnualEntitlement,
    bool CarryForwardAllowed,
    decimal MaxCarryForwardDays) : ICommandResult<LeaveTypeResponse>;
