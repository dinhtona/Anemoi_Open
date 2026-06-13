using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.UpdateInsuranceContributionRule;

public sealed class UpdateInsuranceContributionRuleValidator : AbstractValidator<UpdateInsuranceContributionRuleCommand>
{
    public UpdateInsuranceContributionRuleValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ContributionType).NotEmpty().MaximumLength(64);
        RuleFor(x => x.EmployeeRate).InclusiveBetween(0m, 1m);
        RuleFor(x => x.EmployerRate).InclusiveBetween(0m, 1m);
        RuleFor(x => x).Must(x => x.EmployeeRate > 0 || x.EmployerRate > 0)
            .WithMessage("At least one rate must be greater than zero");
        RuleFor(x => x.SalaryBasis).NotEmpty().MaximumLength(64);
    }
}
