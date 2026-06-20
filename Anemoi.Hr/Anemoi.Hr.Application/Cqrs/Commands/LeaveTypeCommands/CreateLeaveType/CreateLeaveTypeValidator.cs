using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveTypeCommands.CreateLeaveType;

public sealed class CreateLeaveTypeValidator : AbstractValidator<CreateLeaveTypeCommand>
{
    public CreateLeaveTypeValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.AnnualEntitlement).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxCarryForwardDays).GreaterThanOrEqualTo(0);
    }
}
