using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.OvertimeRuleCommands.UpdateOvertimeRule;

public sealed class UpdateOvertimeRuleValidator : AbstractValidator<UpdateOvertimeRuleCommand>
{
    public UpdateOvertimeRuleValidator()
    {
        RuleFor(x => x.Id).NotNull();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.WeekdayMultiplier).GreaterThanOrEqualTo(0);
        RuleFor(x => x.WeekendMultiplier).GreaterThanOrEqualTo(0);
        RuleFor(x => x.HolidayMultiplier).GreaterThanOrEqualTo(0);
    }
}
