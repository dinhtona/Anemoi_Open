using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.DeleteInsuranceContributionRule;

public sealed class DeleteInsuranceContributionRuleValidator : AbstractValidator<DeleteInsuranceContributionRuleCommand>
{
    public DeleteInsuranceContributionRuleValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
