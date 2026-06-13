using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.EssCommands.CancelMyLeaveRequest;

public sealed class CancelMyLeaveRequestValidator : AbstractValidator<CancelMyLeaveRequestCommand>
{
    public CancelMyLeaveRequestValidator()
    {
        RuleFor(x => x.LeaveRequestId).RequiredId("VAL_LEAVE_REQUEST_ID_REQUIRED");
    }
}
