using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.UpdateTaxBracket;

public sealed class UpdateTaxBracketValidator : AbstractValidator<UpdateTaxBracketCommand>
{
    public UpdateTaxBracketValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FromAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Rate).GreaterThanOrEqualTo(0);
    }
}
