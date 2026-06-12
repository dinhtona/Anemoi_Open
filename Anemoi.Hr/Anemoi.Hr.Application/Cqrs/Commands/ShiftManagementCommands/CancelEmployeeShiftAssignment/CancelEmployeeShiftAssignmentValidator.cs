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
            .WithErrorCode("HR_CANCELLED_BY_REQUIRED");
        RuleFor(x => x.CancellationReason)
            .NotEmpty()
            .WithErrorCode("HR_CANCELLATION_REASON_REQUIRED")
            .MaximumLength(500)
            .WithErrorCode("HR_CANCELLATION_REASON_MAX_LENGTH");
    }
}
