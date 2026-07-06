using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.UpdateOnboardingPlanTemplate;

public sealed class UpdateOnboardingPlanTemplateValidator : AbstractValidator<UpdateOnboardingPlanTemplateCommand>
{
    public UpdateOnboardingPlanTemplateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValNameRequired);
        RuleFor(x => x.TaskTemplates)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.HrOnboardingTemplateHasNoTasks);
    }
}
