using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateCandidate;

public sealed class CreateCandidateValidator : AbstractValidator<CreateCandidateCommand>
{
    public CreateCandidateValidator()
    {
        RuleFor(x => x.CandidateCode)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.ValRequisitionCodeRequired);
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.ValCandidateNameRequired);
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.ValCandidateEmailRequired)
            .EmailAddress().WithMessage(HrBusinessErrorCodes.ValRequisitionIdRequired);
        RuleFor(x => x.Source)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.ValRequisitionEmploymentTypeRequired);
    }
}
