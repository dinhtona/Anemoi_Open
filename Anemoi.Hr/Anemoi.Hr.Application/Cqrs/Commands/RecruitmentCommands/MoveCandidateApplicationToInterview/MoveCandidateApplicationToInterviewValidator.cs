using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.MoveCandidateApplicationToInterview;

public sealed class MoveCandidateApplicationToInterviewValidator : AbstractValidator<MoveCandidateApplicationToInterviewCommand>
{
    public MoveCandidateApplicationToInterviewValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValApplicationIdRequired);
    }
}
