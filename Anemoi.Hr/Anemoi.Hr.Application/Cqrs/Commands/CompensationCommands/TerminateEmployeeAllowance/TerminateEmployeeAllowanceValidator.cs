using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.TerminateEmployeeAllowance;

public sealed class TerminateEmployeeAllowanceValidator : AbstractValidator<TerminateEmployeeAllowanceCommand>
{
    public TerminateEmployeeAllowanceValidator()
    {
        RuleFor(x => x.EmployeeAllowanceId).RequiredId(HrBusinessErrorCodes.ValEmployeeAllowanceIdRequired);
        RuleFor(x => x.TerminationDate).NotEmpty();
        
        RuleFor(x => x.SensitivePermissionConfirmed)
            .Equal(true)
            .WithMessage(HrBusinessErrorCodes.PermissionSensitiveConfirmationRequired);
    }
}
