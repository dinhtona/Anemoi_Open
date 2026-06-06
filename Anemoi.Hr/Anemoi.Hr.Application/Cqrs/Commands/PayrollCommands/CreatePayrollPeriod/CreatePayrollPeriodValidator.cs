using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.CreatePayrollPeriod;

public sealed class CreatePayrollPeriodValidator : AbstractValidator<CreatePayrollPeriodCommand>
{
    public CreatePayrollPeriodValidator()
    {
        RuleFor(x => x.PeriodCode)
            .NotEmpty().WithMessage("VAL_PERIOD_CODE_REQUIRED")
            .MaximumLength(64).WithMessage("VAL_PERIOD_CODE_TOO_LONG");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("VAL_START_DATE_REQUIRED");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("VAL_END_DATE_REQUIRED")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("VAL_END_DATE_BEFORE_START_DATE");

        RuleFor(x => x.StandardWorkingDays)
            .GreaterThan(0).WithMessage("VAL_STANDARD_WORKING_DAYS_MUST_BE_POS");
    }
}
