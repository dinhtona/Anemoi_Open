using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.MarkInterviewNoShow;

public sealed class MarkInterviewNoShowValidator : AbstractValidator<MarkInterviewNoShowCommand>
{
    public MarkInterviewNoShowValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValInterviewIdRequired);
    }
}
