using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.RejectLeaveRequest;

public sealed class RejectLeaveRequestValidator : AbstractValidator<RejectLeaveRequestCommand>
{
    public RejectLeaveRequestValidator()
    {
        RuleFor(x => x.Id).RequiredId("VAL_LEAVE_REQUEST_ID_REQUIRED");
        RuleFor(x => x.ApproverEmployeeId).RequiredId("VAL_APPROVER_EMPLOYEE_ID_REQUIRED");
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(1024);
    }
}
