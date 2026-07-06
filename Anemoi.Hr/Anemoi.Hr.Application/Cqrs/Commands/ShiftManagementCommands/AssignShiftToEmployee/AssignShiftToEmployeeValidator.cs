using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.ShiftManagementCommands.AssignShiftToEmployee;

public sealed class AssignShiftToEmployeeValidator : AbstractValidator<AssignShiftToEmployeeCommand>
{
    public AssignShiftToEmployeeValidator()
    {
        RuleFor(x => x.ShiftTemplateId)
            .NotNull()
            .WithErrorCode(HrBusinessErrorCodes.ShiftTemplateNotFound);

        RuleFor(x => x.AssignedBy)
            .NotEmpty()
            .WithErrorCode(HrBusinessErrorCodes.AssignedByRequired);
    }
}
