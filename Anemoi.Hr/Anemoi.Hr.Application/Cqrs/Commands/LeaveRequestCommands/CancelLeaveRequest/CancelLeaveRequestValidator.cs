using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.CancelLeaveRequest;

public sealed class CancelLeaveRequestValidator : AbstractValidator<CancelLeaveRequestCommand>
{
    public CancelLeaveRequestValidator()
    {
        RuleFor(x => x.Id).RequiredId("VAL_LEAVE_REQUEST_ID_REQUIRED");
        RuleFor(x => x.CancelledByEmployeeId).RequiredId("VAL_EMPLOYEE_ID_REQUIRED");
        RuleFor(x => x.Reason).MaximumLength(1024);
    }
}
