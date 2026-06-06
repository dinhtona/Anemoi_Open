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

        RuleFor(x => x.PaidWorkingDays)
            .GreaterThanOrEqualTo(0).WithMessage("VAL_PAID_WORKING_DAYS_MUST_BE_POS");

        RuleFor(x => x.UnpaidLeaveDays)
            .GreaterThanOrEqualTo(0).WithMessage("VAL_UNPAID_LEAVE_DAYS_MUST_BE_POS");
    }
}
