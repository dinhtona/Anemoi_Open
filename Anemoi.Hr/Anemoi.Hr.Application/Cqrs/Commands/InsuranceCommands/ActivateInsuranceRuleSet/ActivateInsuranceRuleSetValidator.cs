using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.ActivateInsuranceRuleSet;

public sealed class ActivateInsuranceRuleSetValidator : AbstractValidator<ActivateInsuranceRuleSetCommand>
{
    public ActivateInsuranceRuleSetValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
