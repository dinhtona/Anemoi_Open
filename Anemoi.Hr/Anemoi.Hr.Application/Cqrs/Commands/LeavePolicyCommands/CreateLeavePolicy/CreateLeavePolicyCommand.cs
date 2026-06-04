using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeavePolicyCommands.CreateLeavePolicy;

public sealed record CreateLeavePolicyCommand(
    string Code,
    string Name,
    string LeaveTypeCode,
    decimal MonthlyAccrualDays,
    decimal AnnualMaxDays,
    bool AllowCarryForward,
    decimal MaxCarryForwardDays,
    bool IsActive) : ICommandResult<LeavePolicyIdResponse>;
