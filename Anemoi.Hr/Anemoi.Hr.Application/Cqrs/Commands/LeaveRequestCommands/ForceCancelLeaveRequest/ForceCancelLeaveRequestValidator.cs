using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.ForceCancelLeaveRequest;

public sealed class ForceCancelLeaveRequestValidator : AbstractValidator<ForceCancelLeaveRequestCommand>
{
    public ForceCancelLeaveRequestValidator()
    {
        RuleFor(x => x.Id).RequiredId("VAL_LEAVE_REQUEST_ID_REQUIRED");
        RuleFor(x => x.ActorEmployeeId).RequiredId("VAL_ACTOR_EMPLOYEE_ID_REQUIRED");
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1024);
        RuleFor(x => x.SensitivePermissionConfirmed)
            .Equal(true)
            .WithMessage(HrBusinessErrorCodes.PermissionSensitiveConfirmationRequired);
    }
}
