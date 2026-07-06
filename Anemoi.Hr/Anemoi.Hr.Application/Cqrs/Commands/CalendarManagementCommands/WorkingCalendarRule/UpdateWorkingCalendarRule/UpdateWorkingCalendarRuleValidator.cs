using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.WorkingCalendarRule.UpdateWorkingCalendarRule;

public sealed class UpdateWorkingCalendarRuleValidator : AbstractValidator<UpdateWorkingCalendarRuleCommand>
{
    public UpdateWorkingCalendarRuleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200)
            .WithErrorCode(HrBusinessErrorCodes.WorkingCalendarRuleNameRequired);

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty()
            .WithErrorCode(HrBusinessErrorCodes.WorkingCalendarRuleEffectiveFromRequired);
    }
}
