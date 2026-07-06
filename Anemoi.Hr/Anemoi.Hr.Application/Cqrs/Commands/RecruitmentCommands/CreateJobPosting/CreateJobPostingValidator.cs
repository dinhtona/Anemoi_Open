using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateJobPosting;

public sealed class CreateJobPostingValidator : AbstractValidator<CreateJobPostingCommand>
{
    public CreateJobPostingValidator()
    {
        RuleFor(x => x.JobRequisitionId)
            .RequiredId(HrBusinessErrorCodes.ValRequisitionIdRequired);
        RuleFor(x => x.PostingTitle)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.ValPostingTitleRequired);
        RuleFor(x => x)
            .Must(x => x.PublishDate <= x.ExpiryDate)
            .WithMessage(HrBusinessErrorCodes.JobPostingDateRangeInvalid);
    }
}
