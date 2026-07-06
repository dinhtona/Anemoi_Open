using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.DeactivateAllowanceType;

public sealed class DeactivateAllowanceTypeValidator : AbstractValidator<DeactivateAllowanceTypeCommand>
{
    public DeactivateAllowanceTypeValidator()
    {
        RuleFor(x => x.AllowanceTypeId).RequiredId(HrBusinessErrorCodes.ValAllowanceTypeIdRequired);

        RuleFor(x => x.SensitivePermissionConfirmed)
            .Equal(true)
            .WithMessage(HrBusinessErrorCodes.PermissionSensitiveConfirmationRequired);
    }
}
