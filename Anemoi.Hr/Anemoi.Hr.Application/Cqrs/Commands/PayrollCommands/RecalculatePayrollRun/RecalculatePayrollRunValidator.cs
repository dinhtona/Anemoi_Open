using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.RecalculatePayrollRun;

public sealed class RecalculatePayrollRunValidator : AbstractValidator<RecalculatePayrollRunCommand>
{
    public RecalculatePayrollRunValidator()
    {
        RuleFor(x => x.PayrollRunId)
            .RequiredId(HrBusinessErrorCodes.ValPayrollRunIdRequired);

        RuleFor(x => x.SensitivePermissionConfirmed)
            .Equal(true)
            .WithMessage(HrBusinessErrorCodes.PermissionSensitiveConfirmationRequired);
    }
}
