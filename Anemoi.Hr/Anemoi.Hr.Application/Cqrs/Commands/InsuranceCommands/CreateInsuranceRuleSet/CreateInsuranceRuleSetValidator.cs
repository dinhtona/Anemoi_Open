using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.CreateInsuranceRuleSet;

public sealed class CreateInsuranceRuleSetValidator : AbstractValidator<CreateInsuranceRuleSetCommand>
{
    public CreateInsuranceRuleSetValidator()
    {
        RuleFor(x => x.CountryCode).NotEmpty().MaximumLength(16);
        RuleFor(x => x.InsuranceType).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(16);
        RuleFor(x => x.EffectiveFrom).NotEmpty();
        When(x => x.EffectiveTo.HasValue, () =>
        {
            RuleFor(x => x.EffectiveTo).GreaterThanOrEqualTo(x => x.EffectiveFrom);
        });
    }
}
