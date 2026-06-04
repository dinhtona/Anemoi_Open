using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeavePolicyCommands.CreateLeavePolicy;

public sealed class CreateLeavePolicyValidator : AbstractValidator<CreateLeavePolicyCommand>
{
    public CreateLeavePolicyValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.LeaveTypeCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.MonthlyAccrualDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AnnualMaxDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxCarryForwardDays).GreaterThanOrEqualTo(0);
    }
}
