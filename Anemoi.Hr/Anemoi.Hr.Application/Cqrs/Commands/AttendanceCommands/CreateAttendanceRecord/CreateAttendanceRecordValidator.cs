using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.AttendanceCommands.CreateAttendanceRecord;

public sealed class CreateAttendanceRecordValidator : AbstractValidator<CreateAttendanceRecordCommand>
{
    public CreateAttendanceRecordValidator()
    {
        RuleFor(x => x.AttendancePeriodId)
            .RequiredId("VAL_ATTENDANCE_PERIOD_ID_REQUIRED");

        RuleFor(x => x.EmployeeId)
            .RequiredId("VAL_EMPLOYEE_ID_REQUIRED");

        RuleFor(x => x.WorkDate)
            .NotEmpty().WithMessage("VAL_WORK_DATE_REQUIRED");

        RuleFor(x => x.WorkedHours)
            .GreaterThanOrEqualTo(0).WithMessage("VAL_WORKED_HOURS_MUST_BE_POS");

        RuleFor(x => x.WorkedDays)
            .GreaterThanOrEqualTo(0).WithMessage("VAL_WORKED_DAYS_MUST_BE_POS");

        RuleFor(x => x.StatusCode)
            .NotEmpty().WithMessage("VAL_STATUS_REQUIRED")
            .MaximumLength(64).WithMessage("VAL_STATUS_TOO_LONG");

        RuleFor(x => x)
            .Must(x => !x.CheckInTime.HasValue || !x.CheckOutTime.HasValue || x.CheckOutTime.Value > x.CheckInTime.Value)
            .WithMessage(HrBusinessErrorCodes.AttendanceInvalidTimeRange);
    }
}
