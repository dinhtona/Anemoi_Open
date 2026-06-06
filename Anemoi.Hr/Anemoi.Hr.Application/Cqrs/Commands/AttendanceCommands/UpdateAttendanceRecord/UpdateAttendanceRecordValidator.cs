using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.AttendanceCommands.UpdateAttendanceRecord;

public sealed class UpdateAttendanceRecordValidator : AbstractValidator<UpdateAttendanceRecordCommand>
{
    public UpdateAttendanceRecordValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId("VAL_ATTENDANCE_RECORD_ID_REQUIRED");

        RuleFor(x => x.AttendancePeriodId)
            .RequiredId("VAL_ATTENDANCE_PERIOD_ID_REQUIRED");

        RuleFor(x => x.EmployeeId)
            .RequiredId("VAL_EMPLOYEE_ID_REQUIRED");

        RuleFor(x => x.WorkDate)
            .NotEmpty().WithMessage("VAL_WORK_DATE_REQUIRED");

        RuleFor(x => x.WorkedHours)
            .InclusiveBetween(0, 24).WithMessage("VAL_WORKED_HOURS_OUT_OF_RANGE");

        RuleFor(x => x.WorkedDays)
            .InclusiveBetween(0, 1).WithMessage("VAL_WORKED_DAYS_OUT_OF_RANGE");

        RuleFor(x => x.StatusCode)
            .NotEmpty().WithMessage("VAL_STATUS_REQUIRED")
            .Must(AttendanceStatusCodes.All.Contains)
            .WithMessage("VAL_ATTENDANCE_STATUS_UNSUPPORTED");

        RuleFor(x => x)
            .Must(x => !x.CheckInTime.HasValue || !x.CheckOutTime.HasValue || x.CheckOutTime.Value > x.CheckInTime.Value)
            .WithMessage(HrBusinessErrorCodes.AttendanceInvalidTimeRange);
    }
}
