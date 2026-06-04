using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeavePolicyCommands.UpdateLeavePolicy;

public sealed record UpdateLeavePolicyCommand(
    LeavePolicyId Id,
    string Name,
    decimal MonthlyAccrualDays,
    decimal AnnualMaxDays,
    bool AllowCarryForward,
    decimal MaxCarryForwardDays,
    bool IsActive) : ICommandVoid;
