using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.MarkInterviewPassed;

public sealed class MarkInterviewPassedValidator : AbstractValidator<MarkInterviewPassedCommand>
{
    public MarkInterviewPassedValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValInterviewIdRequired);
    }
}
