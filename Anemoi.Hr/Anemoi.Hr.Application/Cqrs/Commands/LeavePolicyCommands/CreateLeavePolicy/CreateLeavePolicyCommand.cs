using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeavePolicyCommands.CreateLeavePolicy;

public sealed record CreateLeavePolicyCommand(
    string Code,
    string Name,
    LeaveTypeId LeaveTypeId,
    string ApplicableGradeCode,
    decimal AnnualEntitlement) : ICommandResult<LeavePolicyResponse>;
