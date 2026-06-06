using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.AssignPositionAllowance;

public sealed class AssignPositionAllowanceValidator : AbstractValidator<AssignPositionAllowanceCommand>
{
    public AssignPositionAllowanceValidator()
    {
        RuleFor(x => x.PositionId).RequiredId("VAL_POSITION_ID_REQUIRED");
        RuleFor(x => x.AllowanceTypeId).RequiredId("VAL_ALLOWANCE_TYPE_ID_REQUIRED");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("VAL_AMOUNT_MUST_BE_POSITIVE");
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(16);
        
        RuleFor(x => x.SensitivePermissionConfirmed)
            .Equal(true)
            .WithMessage(HrBusinessErrorCodes.PermissionSensitiveConfirmationRequired);
    }
}
