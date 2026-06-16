using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.MoveCandidateApplicationToOffer;

public sealed class MoveCandidateApplicationToOfferValidator : AbstractValidator<MoveCandidateApplicationToOfferCommand>
{
    public MoveCandidateApplicationToOfferValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValApplicationIdRequired);
    }
}
