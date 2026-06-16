using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.MarkCandidateApplicationHired;

public sealed class MarkCandidateApplicationHiredValidator : AbstractValidator<MarkCandidateApplicationHiredCommand>
{
    public MarkCandidateApplicationHiredValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValApplicationIdRequired);
    }
}
