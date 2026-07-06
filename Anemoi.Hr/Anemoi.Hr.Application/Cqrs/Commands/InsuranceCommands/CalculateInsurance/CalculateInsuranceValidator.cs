using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.CalculateInsurance;

public sealed class CalculateInsuranceValidator : AbstractValidator<CalculateInsuranceCommand>
{
    public CalculateInsuranceValidator()
    {
        RuleFor(x => x.CountryCode).NotEmpty().MaximumLength(16);
        RuleFor(x => x.InsuranceType).NotEmpty().MaximumLength(32);
        RuleFor(x => x.GrossSalarySnapshot).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(16);
        RuleFor(x => x.SourceModule).NotEmpty().MaximumLength(64);
        RuleFor(x => x.PeriodStart).LessThanOrEqualTo(x => x.PeriodEnd);
    }
}
