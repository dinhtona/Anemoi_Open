using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.ChangeSalary;

public sealed class ChangeSalaryValidator : AbstractValidator<ChangeSalaryCommand>
{
    public ChangeSalaryValidator()
    {
        RuleFor(x => x.EmployeeId).RequiredId("VAL_EMPLOYEE_ID_REQUIRED");
        RuleFor(x => x.BaseSalary).GreaterThan(0).WithMessage("VAL_BASE_SALARY_MUST_BE_POSITIVE");
        RuleFor(x => x.Currency).NotEmpty().WithMessage("VAL_CURRENCY_REQUIRED").MaximumLength(16);
        RuleFor(x => x.EffectiveFrom).NotEmpty().WithMessage("VAL_EFFECTIVE_FROM_REQUIRED");
        RuleFor(x => x.SalaryType).IsInEnum().WithMessage("VAL_SALARY_TYPE_INVALID");
        RuleFor(x => x.Reason).IsInEnum().WithMessage("VAL_REASON_INVALID");
        
        RuleFor(x => x.SensitivePermissionConfirmed)
            .Equal(true)
            .WithMessage(HrBusinessErrorCodes.PermissionSensitiveConfirmationRequired);
    }
}
