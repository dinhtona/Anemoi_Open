using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.ActivateTaxRuleSet;

public sealed class ActivateTaxRuleSetValidator : AbstractValidator<ActivateTaxRuleSetCommand>
{
    public ActivateTaxRuleSetValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
