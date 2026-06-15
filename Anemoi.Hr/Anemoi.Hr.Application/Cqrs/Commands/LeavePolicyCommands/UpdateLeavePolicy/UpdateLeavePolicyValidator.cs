using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeavePolicyCommands.UpdateLeavePolicy;

public sealed class UpdateLeavePolicyValidator : AbstractValidator<UpdateLeavePolicyCommand>
{
    public UpdateLeavePolicyValidator()
    {
        RuleFor(x => x.Id).RequiredId(HrBusinessErrorCodes.ValLeavePolicyIdRequired);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.MonthlyAccrualDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AnnualMaxDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxCarryForwardDays).GreaterThanOrEqualTo(0);
    }
}
