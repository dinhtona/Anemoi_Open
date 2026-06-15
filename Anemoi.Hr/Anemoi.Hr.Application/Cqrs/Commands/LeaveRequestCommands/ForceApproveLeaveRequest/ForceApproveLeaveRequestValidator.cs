using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.ForceApproveLeaveRequest;

public sealed class ForceApproveLeaveRequestValidator : AbstractValidator<ForceApproveLeaveRequestCommand>
{
    public ForceApproveLeaveRequestValidator()
    {
        RuleFor(x => x.Id).RequiredId(HrBusinessErrorCodes.ValLeaveRequestIdRequired);
        RuleFor(x => x.ActorEmployeeId).RequiredId(HrBusinessErrorCodes.ValActorEmployeeIdRequired);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1024);
        RuleFor(x => x.SensitivePermissionConfirmed)
            .Equal(true)
            .WithMessage(HrBusinessErrorCodes.PermissionSensitiveConfirmationRequired);
    }
}
