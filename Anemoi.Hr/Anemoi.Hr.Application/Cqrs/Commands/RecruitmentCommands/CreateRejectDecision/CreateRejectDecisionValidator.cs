using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateRejectDecision;

public sealed class CreateRejectDecisionValidator : AbstractValidator<CreateRejectDecisionCommand>
{
    public CreateRejectDecisionValidator()
    {
        RuleFor(x => x.CandidateApplicationId)
            .RequiredId(HrBusinessErrorCodes.ValApplicationIdRequired);
    }
}
