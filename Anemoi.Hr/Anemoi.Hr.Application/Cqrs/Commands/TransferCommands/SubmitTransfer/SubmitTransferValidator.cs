using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.TransferCommands.SubmitTransfer;

public sealed class SubmitTransferValidator : AbstractValidator<SubmitTransferCommand>
{
    public SubmitTransferValidator()
    {
        RuleFor(x => x.EmployeeId)
            .RequiredId(HrBusinessErrorCodes.ValEmployeeIdRequired);
        RuleFor(x => x.ToDepartmentId)
            .RequiredId(HrBusinessErrorCodes.ValNewDepartmentIdRequired);
        RuleFor(x => x.ToPositionId)
            .RequiredId(HrBusinessErrorCodes.ValPositionIdRequired);
        RuleFor(x => x.EffectiveDate)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValStartDateRequired);
        RuleFor(x => x.Reason)
            .NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValReasonInvalid);
    }
}
