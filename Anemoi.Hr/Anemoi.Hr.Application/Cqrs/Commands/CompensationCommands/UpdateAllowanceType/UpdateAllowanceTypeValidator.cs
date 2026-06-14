using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.UpdateAllowanceType;

public sealed class UpdateAllowanceTypeValidator : AbstractValidator<UpdateAllowanceTypeCommand>
{
    public UpdateAllowanceTypeValidator()
    {
        RuleFor(x => x.AllowanceTypeId).RequiredId("VAL_ALLOWANCE_TYPE_ID_REQUIRED");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Description).MaximumLength(512);

        RuleFor(x => x.SensitivePermissionConfirmed)
            .Equal(true)
            .WithMessage(HrBusinessErrorCodes.PermissionSensitiveConfirmationRequired);
    }
}
