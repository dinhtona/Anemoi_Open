using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.CalendarException.UpdateCalendarException;

public sealed class UpdateCalendarExceptionValidator : AbstractValidator<UpdateCalendarExceptionCommand>
{
    public UpdateCalendarExceptionValidator()
    {
        RuleFor(x => x.ExceptionDate)
            .NotEmpty()
            .WithErrorCode(HrBusinessErrorCodes.CalendarExceptionDateRequired);

        RuleFor(x => x.ExceptionType)
            .Must(x => x is "WorkingDayOverride" or "HolidayOverride")
            .WithErrorCode(HrBusinessErrorCodes.CalendarExceptionTypeRequired);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(500)
            .WithErrorCode(HrBusinessErrorCodes.CalendarExceptionReasonRequired);
    }
}
