using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.UpdateTaxDeductionRule;

public sealed class UpdateTaxDeductionRuleValidator : AbstractValidator<UpdateTaxDeductionRuleCommand>
{
    public UpdateTaxDeductionRuleValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
    }
}
