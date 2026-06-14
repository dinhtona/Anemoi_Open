using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.DeactivateAllowanceType;

public sealed class DeactivateAllowanceTypeValidator : AbstractValidator<DeactivateAllowanceTypeCommand>
{
    public DeactivateAllowanceTypeValidator()
    {
        RuleFor(x => x.AllowanceTypeId).RequiredId("VAL_ALLOWANCE_TYPE_ID_REQUIRED");

        RuleFor(x => x.SensitivePermissionConfirmed)
            .Equal(true)
            .WithMessage(HrBusinessErrorCodes.PermissionSensitiveConfirmationRequired);
    }
}
