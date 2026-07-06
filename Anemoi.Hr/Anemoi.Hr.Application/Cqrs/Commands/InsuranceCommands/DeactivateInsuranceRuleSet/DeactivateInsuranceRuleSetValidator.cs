using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.DeactivateInsuranceRuleSet;

public sealed class DeactivateInsuranceRuleSetValidator : AbstractValidator<DeactivateInsuranceRuleSetCommand>
{
    public DeactivateInsuranceRuleSetValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
