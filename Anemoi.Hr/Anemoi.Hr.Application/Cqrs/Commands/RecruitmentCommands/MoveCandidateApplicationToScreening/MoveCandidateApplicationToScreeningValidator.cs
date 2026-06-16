using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.MoveCandidateApplicationToScreening;

public sealed class MoveCandidateApplicationToScreeningValidator : AbstractValidator<MoveCandidateApplicationToScreeningCommand>
{
    public MoveCandidateApplicationToScreeningValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValApplicationIdRequired);
    }
}
