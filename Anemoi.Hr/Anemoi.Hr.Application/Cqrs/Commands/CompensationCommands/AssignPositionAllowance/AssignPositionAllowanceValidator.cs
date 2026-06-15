using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.AssignPositionAllowance;

public sealed class AssignPositionAllowanceValidator : AbstractValidator<AssignPositionAllowanceCommand>
{
    public AssignPositionAllowanceValidator()
    {
        RuleFor(x => x.PositionId).RequiredId(HrBusinessErrorCodes.ValPositionIdRequired);
        RuleFor(x => x.AllowanceTypeId).RequiredId(HrBusinessErrorCodes.ValAllowanceTypeIdRequired);
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage(HrBusinessErrorCodes.ValAmountMustBePositive);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(16);
        
        RuleFor(x => x.SensitivePermissionConfirmed)
            .Equal(true)
            .WithMessage(HrBusinessErrorCodes.PermissionSensitiveConfirmationRequired);
    }
}
