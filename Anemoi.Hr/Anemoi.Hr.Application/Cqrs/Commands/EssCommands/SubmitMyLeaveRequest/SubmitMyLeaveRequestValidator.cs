using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.EssCommands.SubmitMyLeaveRequest;

public sealed class SubmitMyLeaveRequestValidator : AbstractValidator<SubmitMyLeaveRequestCommand>
{
    public SubmitMyLeaveRequestValidator()
    {
        RuleFor(x => x.LeavePolicyId).RequiredId(HrBusinessErrorCodes.ValLeavePolicyIdRequired);
        RuleFor(x => x.LeaveTypeCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.RequestedDays).GreaterThan(0);
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage(HrBusinessErrorCodes.LeaveRequestInvalidDateRange)
            .Must((cmd, endDate) => cmd.StartDate.Year == endDate.Year)
            .WithMessage(HrBusinessErrorCodes.LeaveRequestMultiYearNotSupported);
        RuleFor(x => x.Reason).MaximumLength(1024);
    }
}
