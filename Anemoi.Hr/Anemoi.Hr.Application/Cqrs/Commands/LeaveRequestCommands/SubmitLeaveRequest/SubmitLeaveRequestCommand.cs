using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.SubmitLeaveRequest;

public sealed record SubmitLeaveRequestCommand(
    EmployeeId EmployeeId,
    LeavePolicyId LeavePolicyId,
    string LeaveTypeCode,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal RequestedDays,
    string Reason) : ICommandResult<LeaveRequestIdResponse>;
