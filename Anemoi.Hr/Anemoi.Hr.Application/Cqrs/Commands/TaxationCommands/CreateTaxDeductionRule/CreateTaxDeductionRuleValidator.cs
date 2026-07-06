using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.CreateTaxDeductionRule;

public sealed class CreateTaxDeductionRuleValidator : AbstractValidator<CreateTaxDeductionRuleCommand>
{
    public CreateTaxDeductionRuleValidator()
    {
        RuleFor(x => x.TaxRuleSetId).NotEmpty();
        RuleFor(x => x.DeductionType).NotEmpty();
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
    }
}
