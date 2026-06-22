using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.ProbationCommands.StartProbation;

public sealed class StartProbationValidator : AbstractValidator<StartProbationCommand>
{
    public StartProbationValidator()
    {
        RuleFor(x => x.EmployeeId)
            .RequiredId(HrBusinessErrorCodes.ValEmployeeIdRequired);
        RuleFor(x => x.StartDate)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValStartDateRequired);
        RuleFor(x => x.EndDate)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValEndDateRequired);
        RuleFor(x => x)
            .Must(x => x.EndDate > x.StartDate)
            .WithErrorCode(HrBusinessErrorCodes.ValEndDateBeforeStartDate);
    }
}
