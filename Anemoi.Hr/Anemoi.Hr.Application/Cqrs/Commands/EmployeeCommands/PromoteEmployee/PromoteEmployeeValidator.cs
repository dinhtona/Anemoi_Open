using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.PromoteEmployee;

public sealed class PromoteEmployeeValidator : AbstractValidator<PromoteEmployeeCommand>
{
    public PromoteEmployeeValidator()
    {
        RuleFor(x => x.EmployeeId).RequiredId(HrBusinessErrorCodes.ValEmployeeIdRequired);
        RuleFor(x => x)
            .Must(x => x.NewPositionId is not null || !string.IsNullOrWhiteSpace(x.NewGradeCode))
            .WithMessage(HrBusinessErrorCodes.PromotionNoChange);
        RuleFor(x => x.NewGradeCode)
            .MaximumLength(64)
            .When(x => !string.IsNullOrWhiteSpace(x.NewGradeCode));
        RuleFor(x => x.EffectiveDate).NotEmpty();
        RuleFor(x => x.ReasonCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.SensitivePermissionConfirmed)
            .Equal(true)
            .WithMessage(HrBusinessErrorCodes.PermissionSensitiveConfirmationRequired);
    }
}
