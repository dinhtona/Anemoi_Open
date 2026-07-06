using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CancelRequisition;

public sealed class CancelRequisitionValidator : AbstractValidator<CancelRequisitionCommand>
{
    public CancelRequisitionValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValRequisitionIdRequired);
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.RequisitionCancellationReasonRequired);
    }
}
