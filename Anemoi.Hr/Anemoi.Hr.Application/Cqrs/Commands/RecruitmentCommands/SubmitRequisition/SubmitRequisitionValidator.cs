using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.SubmitRequisition;

public sealed class SubmitRequisitionValidator : AbstractValidator<SubmitRequisitionCommand>
{
    public SubmitRequisitionValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValRequisitionIdRequired);
    }
}
