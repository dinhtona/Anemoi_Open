using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.ProbationCommands.FailProbation;

public sealed class FailProbationValidator : AbstractValidator<FailProbationCommand>
{
    public FailProbationValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValProbationRecordIdRequired!);
        RuleFor(x => x.Comment)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValReasonRequired);
    }
}
