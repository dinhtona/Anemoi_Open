using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.RejectCandidateApplication;

public sealed class RejectCandidateApplicationValidator : AbstractValidator<RejectCandidateApplicationCommand>
{
    public RejectCandidateApplicationValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValApplicationIdRequired);
    }
}
