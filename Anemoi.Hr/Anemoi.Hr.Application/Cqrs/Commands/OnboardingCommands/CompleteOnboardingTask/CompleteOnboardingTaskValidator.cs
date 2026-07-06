using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.CompleteOnboardingTask;

public sealed class CompleteOnboardingTaskValidator : AbstractValidator<CompleteOnboardingTaskCommand>
{
    public CompleteOnboardingTaskValidator()
    {
        RuleFor(x => x.TaskId)
            .RequiredId(HrBusinessErrorCodes.ValTaskIdRequired);
    }
}
