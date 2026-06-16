using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.ReactivateCandidate;

public sealed class ReactivateCandidateValidator : AbstractValidator<ReactivateCandidateCommand>
{
    public ReactivateCandidateValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValCandidateIdRequired);
    }
}
