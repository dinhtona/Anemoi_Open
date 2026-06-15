using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.EssCommands.SubmitMyLeaveRequest;

public sealed record SubmitMyLeaveRequestCommand(
    string? UserId,
    string? Email,
    LeavePolicyId LeavePolicyId,
    EmployeeId ApproverEmployeeId,
    string LeaveTypeCode,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal RequestedDays,
    string Reason) : ICommandResult<LeaveRequestIdResponse>;
