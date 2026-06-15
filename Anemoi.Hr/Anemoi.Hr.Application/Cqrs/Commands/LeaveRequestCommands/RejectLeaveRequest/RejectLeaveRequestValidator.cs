using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.RejectLeaveRequest;

public sealed class RejectLeaveRequestValidator : AbstractValidator<RejectLeaveRequestCommand>
{
    public RejectLeaveRequestValidator()
    {
        RuleFor(x => x.Id).RequiredId(HrBusinessErrorCodes.ValLeaveRequestIdRequired);
        RuleFor(x => x.ApproverEmployeeId).RequiredId(HrBusinessErrorCodes.ValApproverEmployeeIdRequired);
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(1024);
    }
}
