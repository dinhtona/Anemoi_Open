using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.SubmitLeaveRequest;

public sealed class SubmitLeaveRequestValidator : AbstractValidator<SubmitLeaveRequestCommand>
{
    public SubmitLeaveRequestValidator()
    {
        RuleFor(x => x.EmployeeId).RequiredId("VAL_EMPLOYEE_ID_REQUIRED");
        RuleFor(x => x.LeavePolicyId).RequiredId("VAL_LEAVE_POLICY_ID_REQUIRED");
        RuleFor(x => x.ApproverEmployeeId).RequiredId("VAL_APPROVER_EMPLOYEE_ID_REQUIRED");
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
