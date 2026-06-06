using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.CalculatePayrollRun;

public sealed class CalculatePayrollRunValidator : AbstractValidator<CalculatePayrollRunCommand>
{
    public CalculatePayrollRunValidator()
    {
        RuleFor(x => x.PayrollPeriodId)
            .RequiredId("VAL_PAYROLL_PERIOD_ID_REQUIRED");

        RuleFor(x => x.EmployeeId)
            .RequiredId("VAL_EMPLOYEE_ID_REQUIRED");

        RuleFor(x => x.SensitivePermissionConfirmed)
            .Equal(true)
            .WithMessage(HrBusinessErrorCodes.PermissionSensitiveConfirmationRequired);
    }
}
