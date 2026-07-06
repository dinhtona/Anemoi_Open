using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.EssCommands.SubmitMyOvertimeRequest;

public sealed class SubmitMyOvertimeRequestValidator : AbstractValidator<SubmitMyOvertimeRequestCommand>
{
    public SubmitMyOvertimeRequestValidator(HrSettings hrSettings)
    {
        RuleFor(x => x.OvertimeDate)
            .NotNull()
            .Must(x => x >= DateOnly.FromDateTime(DateTime.Today.AddDays(-hrSettings.OvertimeHistoricalDaysLimit)))
            .WithErrorCode(HrBusinessErrorCodes.OvertimeDateInvalid);

        RuleFor(x => x.StartTime)
            .NotNull()
            .Must((x, startTime) => startTime < x.EndTime)
            .WithErrorCode(HrBusinessErrorCodes.OvertimeTimeInvalid);

        RuleFor(x => x.EndTime)
            .NotNull()
            .Must((x, endTime) => endTime > x.StartTime)
            .WithErrorCode(HrBusinessErrorCodes.OvertimeTimeInvalid);

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500)
            .WithErrorCode(HrBusinessErrorCodes.OvertimeReasonInvalid);
    }
}
