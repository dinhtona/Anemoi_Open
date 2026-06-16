using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.ForceCompleteOnboarding;

public sealed class ForceCompleteOnboardingValidator : AbstractValidator<ForceCompleteOnboardingCommand>
{
    public ForceCompleteOnboardingValidator()
    {
        RuleFor(x => x.InstanceId)
            .RequiredId(HrBusinessErrorCodes.ValInstanceIdRequired);
        RuleFor(x => x.Reason)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValReasonRequired);
    }
}
