using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateOfferDecision;

public sealed class CreateOfferDecisionValidator : AbstractValidator<CreateOfferDecisionCommand>
{
    public CreateOfferDecisionValidator()
    {
        RuleFor(x => x.CandidateApplicationId)
            .RequiredId(HrBusinessErrorCodes.ValApplicationIdRequired);
    }
}
