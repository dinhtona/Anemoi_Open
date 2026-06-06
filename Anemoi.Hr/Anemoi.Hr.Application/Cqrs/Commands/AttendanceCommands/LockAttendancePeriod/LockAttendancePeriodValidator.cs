using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.AttendanceCommands.LockAttendancePeriod;

public sealed class LockAttendancePeriodValidator : AbstractValidator<LockAttendancePeriodCommand>
{
    public LockAttendancePeriodValidator()
    {
        RuleFor(x => x.AttendancePeriodId)
            .RequiredId("VAL_ATTENDANCE_PERIOD_ID_REQUIRED");

        RuleFor(x => x.SensitivePermissionConfirmed)
            .Equal(true)
            .WithMessage(HrBusinessErrorCodes.PermissionSensitiveConfirmationRequired);
    }
}
