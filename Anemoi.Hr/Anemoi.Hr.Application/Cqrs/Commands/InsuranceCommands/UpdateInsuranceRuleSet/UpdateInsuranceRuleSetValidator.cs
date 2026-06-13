using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.UpdateInsuranceRuleSet;

public sealed class UpdateInsuranceRuleSetValidator : AbstractValidator<UpdateInsuranceRuleSetCommand>
{
    public UpdateInsuranceRuleSetValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(16);
        RuleFor(x => x.EffectiveFrom).NotEmpty();
        When(x => x.EffectiveTo.HasValue, () =>
        {
            RuleFor(x => x.EffectiveTo).GreaterThanOrEqualTo(x => x.EffectiveFrom);
        });
    }
}
