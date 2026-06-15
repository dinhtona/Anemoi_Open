using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveBalanceCommands.AdjustLeaveBalance;

public sealed class AdjustLeaveBalanceValidator : AbstractValidator<AdjustLeaveBalanceCommand>
{
    public AdjustLeaveBalanceValidator()
    {
        RuleFor(x => x.EmployeeId).RequiredId(HrBusinessErrorCodes.ValEmployeeIdRequired);
        RuleFor(x => x.LeavePolicyId).RequiredId(HrBusinessErrorCodes.ValLeavePolicyIdRequired);
        RuleFor(x => x.ActorEmployeeId).RequiredId(HrBusinessErrorCodes.ValActorEmployeeIdRequired);
        RuleFor(x => x.Year).InclusiveBetween(1900, 9999);
        RuleFor(x => x.Days).NotEqual(0);
        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage(HrBusinessErrorCodes.LeaveAdjustmentReasonRequired)
            .MaximumLength(1024);
        RuleFor(x => x.SourceType).MaximumLength(64);
        RuleFor(x => x.SourceId).MaximumLength(128);
        RuleFor(x => x.SensitivePermissionConfirmed)
            .Equal(true)
            .WithMessage(HrBusinessErrorCodes.PermissionSensitiveConfirmationRequired);
    }
}
