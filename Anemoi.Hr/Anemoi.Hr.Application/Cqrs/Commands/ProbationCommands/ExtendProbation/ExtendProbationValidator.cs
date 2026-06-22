using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.ProbationCommands.ExtendProbation;

public sealed class ExtendProbationValidator : AbstractValidator<ExtendProbationCommand>
{
    public ExtendProbationValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValProbationRecordIdRequired!);
        RuleFor(x => x.NewEndDate)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValNewEndDateRequired!);
    }
}
