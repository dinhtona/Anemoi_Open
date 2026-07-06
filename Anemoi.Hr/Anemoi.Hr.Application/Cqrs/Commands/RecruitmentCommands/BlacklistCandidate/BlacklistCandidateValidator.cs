using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.BlacklistCandidate;

public sealed class BlacklistCandidateValidator : AbstractValidator<BlacklistCandidateCommand>
{
    public BlacklistCandidateValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValCandidateIdRequired);
    }
}
