using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.ApproveLeaveRequest;

public sealed class ApproveLeaveRequestValidator : AbstractValidator<ApproveLeaveRequestCommand>
{
    public ApproveLeaveRequestValidator()
    {
        RuleFor(x => x.Id).RequiredId("VAL_LEAVE_REQUEST_ID_REQUIRED");
        RuleFor(x => x.ApproverEmployeeId).RequiredId("VAL_APPROVER_EMPLOYEE_ID_REQUIRED");
        RuleFor(x => x.Comment).MaximumLength(1024);
    }
}
