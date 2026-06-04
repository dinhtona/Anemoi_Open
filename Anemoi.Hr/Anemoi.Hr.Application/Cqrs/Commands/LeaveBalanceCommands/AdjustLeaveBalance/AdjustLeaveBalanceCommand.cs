using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveBalanceCommands.AdjustLeaveBalance;

public sealed record AdjustLeaveBalanceCommand(
    EmployeeId EmployeeId,
    LeavePolicyId LeavePolicyId,
    int Year,
    decimal Days,
    string Reason,
    string SourceType,
    string SourceId,
    EmployeeId ActorEmployeeId,
    bool SensitivePermissionConfirmed) : ICommandVoid;
