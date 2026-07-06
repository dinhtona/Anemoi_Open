using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.CreateOnboardingPlanTemplate;

public sealed class CreateOnboardingPlanTemplateValidator : AbstractValidator<CreateOnboardingPlanTemplateCommand>
{
    public CreateOnboardingPlanTemplateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValNameRequired);
        RuleFor(x => x.TaskTemplates)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.HrOnboardingTemplateHasNoTasks);
        RuleForEach(x => x.TaskTemplates).ChildRules(task =>
        {
            task.RuleFor(t => t.Title)
                .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValTitleRequired);
        });
    }
}
