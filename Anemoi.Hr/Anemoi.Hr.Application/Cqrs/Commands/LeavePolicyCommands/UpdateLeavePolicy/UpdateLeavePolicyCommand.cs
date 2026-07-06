using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeavePolicyCommands.UpdateLeavePolicy;

public sealed record UpdateLeavePolicyCommand(
    LeavePolicyId Id,
    string Code,
    string Name,
    LeaveTypeId LeaveTypeId,
    string ApplicableGradeCode,
    decimal AnnualEntitlement) : ICommandResult<LeavePolicyResponse>;
