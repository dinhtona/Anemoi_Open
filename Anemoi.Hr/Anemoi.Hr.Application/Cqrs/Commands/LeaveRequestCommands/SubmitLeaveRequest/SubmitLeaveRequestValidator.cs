using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.SubmitLeaveRequest;

public sealed class SubmitLeaveRequestValidator : AbstractValidator<SubmitLeaveRequestCommand>
{
    public SubmitLeaveRequestValidator()
    {
        RuleFor(x => x.EmployeeId).RequiredId(HrBusinessErrorCodes.ValEmployeeIdRequired);
        RuleFor(x => x.LeavePolicyId).RequiredId(HrBusinessErrorCodes.ValLeavePolicyIdRequired);
        RuleFor(x => x.ApproverEmployeeId).RequiredId(HrBusinessErrorCodes.ValApproverEmployeeIdRequired);
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
