using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.CalculateTax;

public sealed class CalculateTaxValidator : AbstractValidator<CalculateTaxCommand>
{
    public CalculateTaxValidator()
    {
        RuleFor(x => x.CountryCode).NotEmpty().MaximumLength(16);
        RuleFor(x => x.TaxType).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(16);
        RuleFor(x => x.GrossIncome).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TaxableIncome).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SourceModule).NotEmpty().MaximumLength(64);
        RuleFor(x => x.PeriodStart).LessThanOrEqualTo(x => x.PeriodEnd);
    }
}
