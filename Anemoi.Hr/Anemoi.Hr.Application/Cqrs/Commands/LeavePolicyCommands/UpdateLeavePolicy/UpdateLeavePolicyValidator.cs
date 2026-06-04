using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeavePolicyCommands.UpdateLeavePolicy;

public sealed class UpdateLeavePolicyValidator : AbstractValidator<UpdateLeavePolicyCommand>
{
    public UpdateLeavePolicyValidator()
    {
        RuleFor(x => x.Id).RequiredId("VAL_LEAVE_POLICY_ID_REQUIRED");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.MonthlyAccrualDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AnnualMaxDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxCarryForwardDays).GreaterThanOrEqualTo(0);
    }
}
