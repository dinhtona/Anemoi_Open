using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.MarkInterviewFailed;

public sealed class MarkInterviewFailedValidator : AbstractValidator<MarkInterviewFailedCommand>
{
    public MarkInterviewFailedValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValInterviewIdRequired);
    }
}
