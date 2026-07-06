using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.ProbationCommands.PassProbation;

public sealed class PassProbationValidator : AbstractValidator<PassProbationCommand>
{
    public PassProbationValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValProbationRecordIdRequired!);
        RuleFor(x => x.Result)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValProbationResultRequired!);
    }
}
