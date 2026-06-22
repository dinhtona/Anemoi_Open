using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.SeparationCommands.SubmitSeparation;

public sealed class SubmitSeparationValidator : AbstractValidator<SubmitSeparationCommand>
{
    public SubmitSeparationValidator()
    {
        RuleFor(x => x.EmployeeId)
            .RequiredId(HrBusinessErrorCodes.ValEmployeeIdRequired);
        RuleFor(x => x.SeparationType)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValSeparationTypeRequired!);
        RuleFor(x => x.Reason)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValReasonInvalid);
        RuleFor(x => x.LastWorkingDate)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValLastWorkingDateRequired!);
    }
}
