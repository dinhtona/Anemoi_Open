using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.StartOnboarding;

public sealed class StartOnboardingValidator : AbstractValidator<StartOnboardingCommand>
{
    public StartOnboardingValidator()
    {
        RuleFor(x => x.EmployeeId)
            .RequiredId(HrBusinessErrorCodes.ValEmployeeIdRequired);
        RuleFor(x => x.TemplateId)
            .RequiredId(HrBusinessErrorCodes.ValTemplateIdRequired);
        RuleFor(x => x.StartDate)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValStartDateRequired);
    }
}
