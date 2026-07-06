using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.AttendanceCommands.CreateAttendancePeriod;

public sealed class CreateAttendancePeriodValidator : AbstractValidator<CreateAttendancePeriodCommand>
{
    public CreateAttendancePeriodValidator()
    {
        RuleFor(x => x.PeriodCode)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.ValPeriodCodeRequired)
            .MaximumLength(64).WithMessage(HrBusinessErrorCodes.ValPeriodCodeTooLong);

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.ValStartDateRequired);

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.ValEndDateRequired)
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage(HrBusinessErrorCodes.ValEndDateBeforeStartDate);
    }
}
