using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.ExpireJobPosting;

public sealed class ExpireJobPostingValidator : AbstractValidator<ExpireJobPostingCommand>
{
    public ExpireJobPostingValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValPostingIdRequired);
    }
}
