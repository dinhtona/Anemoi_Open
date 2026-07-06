using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.ApproveLeaveRequest;

public sealed class ApproveLeaveRequestValidator : AbstractValidator<ApproveLeaveRequestCommand>
{
    public ApproveLeaveRequestValidator()
    {
        RuleFor(x => x.ApproverEmployeeId).RequiredId(HrBusinessErrorCodes.ValApproverEmployeeIdRequired);
        RuleFor(x => x.Comment).MaximumLength(1024);
    }
}
