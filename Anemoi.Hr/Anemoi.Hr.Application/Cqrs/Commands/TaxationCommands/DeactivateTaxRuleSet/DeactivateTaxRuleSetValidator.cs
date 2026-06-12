using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.DeactivateTaxRuleSet;

public sealed class DeactivateTaxRuleSetValidator : AbstractValidator<DeactivateTaxRuleSetCommand>
{
    public DeactivateTaxRuleSetValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
