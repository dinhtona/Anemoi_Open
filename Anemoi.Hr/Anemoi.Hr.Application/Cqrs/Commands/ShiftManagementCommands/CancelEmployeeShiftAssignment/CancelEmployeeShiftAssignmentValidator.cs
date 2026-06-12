using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.ShiftManagementCommands.CancelEmployeeShiftAssignment;

public sealed class CancelEmployeeShiftAssignmentValidator : AbstractValidator<CancelEmployeeShiftAssignmentCommand>
{
    public CancelEmployeeShiftAssignmentValidator()
    {
        RuleFor(x => x.CancelledBy)
            .NotEmpty()
            .WithErrorCode(HrBusinessErrorCodes.CancelledByRequired);
        RuleFor(x => x.CancellationReason)
            .NotEmpty()
            .WithErrorCode(HrBusinessErrorCodes.CancellationReasonRequired)
            .MaximumLength(500)
            .WithErrorCode(HrBusinessErrorCodes.CancellationReasonMaxLength);
    }
}
