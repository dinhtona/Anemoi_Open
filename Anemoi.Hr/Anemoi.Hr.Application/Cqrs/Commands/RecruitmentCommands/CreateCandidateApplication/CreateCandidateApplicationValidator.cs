using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateCandidateApplication;

public sealed class CreateCandidateApplicationValidator : AbstractValidator<CreateCandidateApplicationCommand>
{
    public CreateCandidateApplicationValidator()
    {
        RuleFor(x => x.CandidateId)
            .RequiredId(HrBusinessErrorCodes.ValCandidateIdRequired);
        RuleFor(x => x.JobPostingId)
            .RequiredId(HrBusinessErrorCodes.ValPostingIdRequired);
    }
}
