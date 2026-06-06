using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.LockPayrollPeriod;

public sealed class LockPayrollPeriodValidator : AbstractValidator<LockPayrollPeriodCommand>
{
    public LockPayrollPeriodValidator()
    {
        RuleFor(x => x.PayrollPeriodId)
            .RequiredId("VAL_PAYROLL_PERIOD_ID_REQUIRED");

        RuleFor(x => x.SensitivePermissionConfirmed)
            .Equal(true)
            .WithMessage(HrBusinessErrorCodes.PermissionSensitiveConfirmationRequired);
    }
}
