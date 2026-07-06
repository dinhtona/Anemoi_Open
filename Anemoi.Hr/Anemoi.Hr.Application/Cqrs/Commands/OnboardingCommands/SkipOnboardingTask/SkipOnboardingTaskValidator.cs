using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.SkipOnboardingTask;

public sealed class SkipOnboardingTaskValidator : AbstractValidator<SkipOnboardingTaskCommand>
{
    public SkipOnboardingTaskValidator()
    {
        RuleFor(x => x.TaskId)
            .RequiredId(HrBusinessErrorCodes.ValTaskIdRequired);
    }
}
