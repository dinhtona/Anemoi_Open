using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.CancelLeaveRequest;

public sealed class CancelLeaveRequestValidator : AbstractValidator<CancelLeaveRequestCommand>
{
    public CancelLeaveRequestValidator()
    {
        RuleFor(x => x.CancelledByEmployeeId).RequiredId(HrBusinessErrorCodes.ValEmployeeIdRequired);
        RuleFor(x => x.Reason).MaximumLength(1024);
    }
}
