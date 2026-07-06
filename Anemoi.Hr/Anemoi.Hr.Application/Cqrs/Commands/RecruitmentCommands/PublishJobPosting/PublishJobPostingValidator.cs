using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.PublishJobPosting;

public sealed class PublishJobPostingValidator : AbstractValidator<PublishJobPostingCommand>
{
    public PublishJobPostingValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValPostingIdRequired);
    }
}
