using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CloseJobPosting;

public sealed class CloseJobPostingValidator : AbstractValidator<CloseJobPostingCommand>
{
    public CloseJobPostingValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValPostingIdRequired);
    }
}
