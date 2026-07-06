using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.UpdateJobPosting;

public sealed class UpdateJobPostingValidator : AbstractValidator<UpdateJobPostingCommand>
{
    public UpdateJobPostingValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValPostingIdRequired);
        RuleFor(x => x.PostingTitle)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.ValPostingTitleRequired);
        RuleFor(x => x)
            .Must(x => x.PublishDate <= x.ExpiryDate)
            .WithMessage(HrBusinessErrorCodes.JobPostingDateRangeInvalid);
    }
}
