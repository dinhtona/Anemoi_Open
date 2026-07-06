using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateInterviewSchedule;

public sealed class CreateInterviewScheduleValidator : AbstractValidator<CreateInterviewScheduleCommand>
{
    public CreateInterviewScheduleValidator()
    {
        RuleFor(x => x.CandidateApplicationId)
            .RequiredId(HrBusinessErrorCodes.ValApplicationIdRequired);
        RuleFor(x => x.InterviewType)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.ValRequisitionEmploymentTypeRequired);
        RuleFor(x => x.ScheduledAt)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.ValStartDateRequired);
        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage(HrBusinessErrorCodes.InterviewDurationInvalid);
        RuleFor(x => x.InterviewerEmployeeId)
            .RequiredId(HrBusinessErrorCodes.ValEmployeeIdRequired);
    }
}
