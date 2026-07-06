using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeavePolicyCommands.UpdateLeavePolicy;

public sealed class UpdateLeavePolicyValidator : AbstractValidator<UpdateLeavePolicyCommand>
{
    public UpdateLeavePolicyValidator()
    {
        RuleFor(x => x.Id).NotNull();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.LeaveTypeId).NotNull();
        RuleFor(x => x.AnnualEntitlement).GreaterThanOrEqualTo(0);
    }
}
