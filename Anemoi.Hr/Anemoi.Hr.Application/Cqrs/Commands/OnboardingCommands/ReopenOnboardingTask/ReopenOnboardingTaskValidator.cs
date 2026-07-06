using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.ReopenOnboardingTask;

public sealed class ReopenOnboardingTaskValidator : AbstractValidator<ReopenOnboardingTaskCommand>
{
    public ReopenOnboardingTaskValidator()
    {
        RuleFor(x => x.TaskId)
            .RequiredId(HrBusinessErrorCodes.ValTaskIdRequired);
    }
}
