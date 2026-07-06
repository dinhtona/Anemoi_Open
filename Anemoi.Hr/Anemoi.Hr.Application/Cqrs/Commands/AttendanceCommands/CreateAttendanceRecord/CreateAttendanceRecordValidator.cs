using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.AttendanceCommands.CreateAttendanceRecord;

public sealed class CreateAttendanceRecordValidator : AbstractValidator<CreateAttendanceRecordCommand>
{
    public CreateAttendanceRecordValidator()
    {
        RuleFor(x => x.AttendancePeriodId)
            .RequiredId(HrBusinessErrorCodes.ValAttendancePeriodIdRequired);

        RuleFor(x => x.EmployeeId)
            .RequiredId(HrBusinessErrorCodes.ValEmployeeIdRequired);

        RuleFor(x => x.WorkDate)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.ValWorkDateRequired);

        RuleFor(x => x.WorkedHours)
            .InclusiveBetween(0, 24).WithMessage(HrBusinessErrorCodes.ValWorkedHoursOutOfRange);

        RuleFor(x => x.WorkedDays)
            .InclusiveBetween(0, 1).WithMessage(HrBusinessErrorCodes.ValWorkedDaysOutOfRange);

        RuleFor(x => x.StatusCode)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.ValStatusRequired)
            .Must(AttendanceStatusCodes.All.Contains)
            .WithMessage(HrBusinessErrorCodes.ValAttendanceStatusUnsupported);

        RuleFor(x => x)
            .Must(x => !x.CheckInTime.HasValue || !x.CheckOutTime.HasValue || x.CheckOutTime.Value > x.CheckInTime.Value)
            .WithMessage(HrBusinessErrorCodes.AttendanceInvalidTimeRange);
    }
}
