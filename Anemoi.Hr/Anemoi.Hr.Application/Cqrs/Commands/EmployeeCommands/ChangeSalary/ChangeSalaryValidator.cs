using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.ChangeSalary;

public sealed class ChangeSalaryValidator : AbstractValidator<ChangeSalaryCommand>
{
    public ChangeSalaryValidator()
    {
        RuleFor(x => x.EmployeeId).RequiredId(HrBusinessErrorCodes.ValEmployeeIdRequired);
        RuleFor(x => x.BaseSalary).GreaterThan(0).WithMessage(HrBusinessErrorCodes.ValBaseSalaryMustBePositive);
        RuleFor(x => x.Currency).NotEmpty().WithMessage(HrBusinessErrorCodes.ValCurrencyRequired).MaximumLength(16);
        RuleFor(x => x.EffectiveFrom).NotEmpty().WithMessage(HrBusinessErrorCodes.ValEffectiveFromRequired);
        RuleFor(x => x.SalaryType).IsInEnum().WithMessage(HrBusinessErrorCodes.ValSalaryTypeInvalid);
        RuleFor(x => x.Reason).IsInEnum().WithMessage(HrBusinessErrorCodes.ValReasonInvalid);
        
        RuleFor(x => x.SensitivePermissionConfirmed)
            .Equal(true)
            .WithMessage(HrBusinessErrorCodes.PermissionSensitiveConfirmationRequired);
    }
}
