using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateHireDecision;

public sealed class CreateHireDecisionValidator : AbstractValidator<CreateHireDecisionCommand>
{
    public CreateHireDecisionValidator()
    {
        RuleFor(x => x.CandidateApplicationId)
            .RequiredId(HrBusinessErrorCodes.ValApplicationIdRequired);
    }
}
