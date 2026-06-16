using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.ArchiveCandidate;

public sealed class ArchiveCandidateValidator : AbstractValidator<ArchiveCandidateCommand>
{
    public ArchiveCandidateValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValCandidateIdRequired);
    }
}
