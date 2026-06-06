using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.RecalculatePayrollRun;

public sealed class RecalculatePayrollRunValidator : AbstractValidator<RecalculatePayrollRunCommand>
{
    public RecalculatePayrollRunValidator()
    {
        RuleFor(x => x.PayrollRunId)
            .RequiredId("VAL_PAYROLL_RUN_ID_REQUIRED");

        RuleFor(x => x.SensitivePermissionConfirmed)
            .Equal(true)
            .WithMessage(HrBusinessErrorCodes.PermissionSensitiveConfirmationRequired);
    }
}
