using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.OvertimeRuleCommands.CreateOvertimeRule;

public sealed class CreateOvertimeRuleValidator : AbstractValidator<CreateOvertimeRuleCommand>
{
    public CreateOvertimeRuleValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.WeekdayMultiplier).GreaterThanOrEqualTo(0);
        RuleFor(x => x.WeekendMultiplier).GreaterThanOrEqualTo(0);
        RuleFor(x => x.HolidayMultiplier).GreaterThanOrEqualTo(0);
    }
}
