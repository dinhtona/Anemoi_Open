using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.UpdateCandidate;

public sealed class UpdateCandidateValidator : AbstractValidator<UpdateCandidateCommand>
{
    public UpdateCandidateValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValCandidateIdRequired);
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.ValCandidateNameRequired);
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.ValCandidateEmailRequired)
            .EmailAddress().WithMessage(HrBusinessErrorCodes.ValRequisitionIdRequired);
    }
}
