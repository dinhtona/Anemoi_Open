using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CloseRequisition;

public sealed class CloseRequisitionValidator : AbstractValidator<CloseRequisitionCommand>
{
    public CloseRequisitionValidator()
    {
        RuleFor(x => x.Id)
            .RequiredId(HrBusinessErrorCodes.ValRequisitionIdRequired);
    }
}
