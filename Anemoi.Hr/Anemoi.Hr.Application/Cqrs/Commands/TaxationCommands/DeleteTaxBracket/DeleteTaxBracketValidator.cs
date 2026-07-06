using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.DeleteTaxBracket;

public sealed class DeleteTaxBracketValidator : AbstractValidator<DeleteTaxBracketCommand>
{
    public DeleteTaxBracketValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
