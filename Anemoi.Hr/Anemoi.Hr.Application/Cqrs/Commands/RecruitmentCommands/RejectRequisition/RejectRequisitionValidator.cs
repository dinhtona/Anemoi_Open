using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.RejectRequisition;

public sealed class RejectRequisitionValidator : AbstractValidator<RejectRequisitionCommand>
{
    public RejectRequisitionValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValRequisitionIdRequired);
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage(HrBusinessErrorCodes.RequisitionRejectionReasonRequired);
    }
}
