using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.CreateTaxRuleSet;

public sealed class CreateTaxRuleSetValidator : AbstractValidator<CreateTaxRuleSetCommand>
{
    public CreateTaxRuleSetValidator()
    {
        RuleFor(x => x.CountryCode).NotEmpty().MaximumLength(16);
        RuleFor(x => x.TaxType).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.EffectiveFrom).NotEmpty();
    }
}
