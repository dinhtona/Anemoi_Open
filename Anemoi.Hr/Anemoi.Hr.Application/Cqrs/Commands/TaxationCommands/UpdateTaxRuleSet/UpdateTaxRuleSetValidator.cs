using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.UpdateTaxRuleSet;

public sealed class CreateTaxRuleSetValidator : AbstractValidator<UpdateTaxRuleSetCommand>
{
    public CreateTaxRuleSetValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.EffectiveFrom).NotEmpty();
    }
}
