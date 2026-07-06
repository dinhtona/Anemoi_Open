using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.WithdrawCandidateApplication;

public sealed class WithdrawCandidateApplicationValidator : AbstractValidator<WithdrawCandidateApplicationCommand>
{
    public WithdrawCandidateApplicationValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValApplicationIdRequired);
    }
}
