using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.ReassignOnboardingTask;

public sealed class ReassignOnboardingTaskValidator : AbstractValidator<ReassignOnboardingTaskCommand>
{
    public ReassignOnboardingTaskValidator()
    {
        RuleFor(x => x.TaskId)
            .RequiredId(HrBusinessErrorCodes.ValTaskIdRequired);
        RuleFor(x => x.NewUserId)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValNewUserIdRequired);
    }
}
