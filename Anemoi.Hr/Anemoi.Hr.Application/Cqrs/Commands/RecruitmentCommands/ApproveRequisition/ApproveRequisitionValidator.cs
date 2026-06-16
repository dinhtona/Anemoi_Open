using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.ApproveRequisition;

public sealed class ApproveRequisitionValidator : AbstractValidator<ApproveRequisitionCommand>
{
    public ApproveRequisitionValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValRequisitionIdRequired);
    }
}
