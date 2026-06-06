using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.AssignEmployeeAllowance;

public sealed class AssignEmployeeAllowanceValidator : AbstractValidator<AssignEmployeeAllowanceCommand>
{
    public AssignEmployeeAllowanceValidator()
    {
        RuleFor(x => x.EmployeeId).RequiredId("VAL_EMPLOYEE_ID_REQUIRED");
        RuleFor(x => x.AllowanceTypeId).RequiredId("VAL_ALLOWANCE_TYPE_ID_REQUIRED");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("VAL_AMOUNT_MUST_BE_POSITIVE");
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(16);
        RuleFor(x => x.EffectiveFrom).NotEmpty();
        RuleFor(x => x.EffectiveTo)
            .GreaterThanOrEqualTo(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue)
            .WithMessage("HR_ALLOWANCE_INVALID_DATE_RANGE");
        
        RuleFor(x => x.SensitivePermissionConfirmed)
            .Equal(true)
            .WithMessage(HrBusinessErrorCodes.PermissionSensitiveConfirmationRequired);
    }
}
