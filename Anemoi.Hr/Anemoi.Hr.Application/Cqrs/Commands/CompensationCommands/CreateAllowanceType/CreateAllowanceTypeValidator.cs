using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.CreateAllowanceType;

public sealed class CreateAllowanceTypeValidator : AbstractValidator<CreateAllowanceTypeCommand>
{
    public CreateAllowanceTypeValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Description).MaximumLength(512);
        
        RuleFor(x => x.SensitivePermissionConfirmed)
            .Equal(true)
            .WithMessage(HrBusinessErrorCodes.PermissionSensitiveConfirmationRequired);
    }
}
