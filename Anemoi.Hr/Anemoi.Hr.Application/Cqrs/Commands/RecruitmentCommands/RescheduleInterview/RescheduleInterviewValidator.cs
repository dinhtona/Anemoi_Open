using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.RescheduleInterview;

public sealed class RescheduleInterviewValidator : AbstractValidator<RescheduleInterviewCommand>
{
    public RescheduleInterviewValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValInterviewIdRequired);
        RuleFor(x => x.ScheduledAt)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.ValStartDateRequired);
        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage(HrBusinessErrorCodes.InterviewDurationInvalid);
    }
}
