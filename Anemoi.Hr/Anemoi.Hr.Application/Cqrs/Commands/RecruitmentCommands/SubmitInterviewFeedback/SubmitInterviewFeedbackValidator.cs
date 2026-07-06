using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.SubmitInterviewFeedback;

public sealed class SubmitInterviewFeedbackValidator : AbstractValidator<SubmitInterviewFeedbackCommand>
{
    public SubmitInterviewFeedbackValidator()
    {
        RuleFor(x => x.InterviewScheduleId)
            .RequiredId(HrBusinessErrorCodes.ValInterviewIdRequired);
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage(HrBusinessErrorCodes.ValAmountMustBePositive);
        RuleFor(x => x.Recommendation)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.ValReasonInvalid);
    }
}
