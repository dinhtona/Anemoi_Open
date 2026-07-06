using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.AssignEmployeeAllowance;

public sealed class AssignEmployeeAllowanceValidator : AbstractValidator<AssignEmployeeAllowanceCommand>
{
    public AssignEmployeeAllowanceValidator()
    {
        RuleFor(x => x.EmployeeId).RequiredId(HrBusinessErrorCodes.ValEmployeeIdRequired);
        RuleFor(x => x.AllowanceTypeId).RequiredId(HrBusinessErrorCodes.ValAllowanceTypeIdRequired);
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage(HrBusinessErrorCodes.ValAmountMustBePositive);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(16);
        RuleFor(x => x.EffectiveFrom).NotEmpty();
        RuleFor(x => x.EffectiveTo)
            .GreaterThanOrEqualTo(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue)
            .WithMessage(HrBusinessErrorCodes.AllowanceInvalidDateRange);
        
        RuleFor(x => x.SensitivePermissionConfirmed)
            .Equal(true)
            .WithMessage(HrBusinessErrorCodes.PermissionSensitiveConfirmationRequired);
    }
}
