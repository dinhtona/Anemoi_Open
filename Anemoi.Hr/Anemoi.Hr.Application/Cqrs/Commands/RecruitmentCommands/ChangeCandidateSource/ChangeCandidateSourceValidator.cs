using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.ChangeCandidateSource;

public sealed class ChangeCandidateSourceValidator : AbstractValidator<ChangeCandidateSourceCommand>
{
    public ChangeCandidateSourceValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValCandidateIdRequired);
        RuleFor(x => x.Source)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.ValRequisitionEmploymentTypeRequired);
    }
}
