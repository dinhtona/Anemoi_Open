using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.CreateTaxBracket;

public sealed class CreateTaxBracketValidator : AbstractValidator<CreateTaxBracketCommand>
{
    public CreateTaxBracketValidator()
    {
        RuleFor(x => x.TaxRuleSetId).NotEmpty();
        RuleFor(x => x.FromAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Rate).GreaterThanOrEqualTo(0);
    }
}
