using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.ShiftManagementCommands.BulkAssignShift;

public sealed class BulkAssignShiftValidator : AbstractValidator<BulkAssignShiftCommand>
{
    public BulkAssignShiftValidator()
    {
        RuleFor(x => x.EmployeeIds)
            .NotEmpty()
            .WithErrorCode(HrBusinessErrorCodes.EmployeeNotFound);

        RuleFor(x => x.ShiftTemplateId)
            .NotNull()
            .WithErrorCode(HrBusinessErrorCodes.ShiftTemplateNotFound);
    }
}
