using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.ShiftManagementCommands.UpdateShiftTemplate;

public sealed class UpdateShiftTemplateValidator : AbstractValidator<UpdateShiftTemplateCommand>
{
    public UpdateShiftTemplateValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50)
            .WithErrorCode(HrBusinessErrorCodes.ShiftTemplateCodeRequired);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200)
            .WithErrorCode(HrBusinessErrorCodes.ShiftTemplateNameRequired);

        RuleFor(x => x.StartTime)
            .NotNull()
            .Must((x, startTime) => startTime != x.EndTime)
            .WithErrorCode(HrBusinessErrorCodes.ShiftTemplateInvalidTimeRange);

        RuleFor(x => x.EndTime)
            .NotNull()
            .Must((x, endTime) => endTime != x.StartTime)
            .WithErrorCode(HrBusinessErrorCodes.ShiftTemplateInvalidTimeRange);

        RuleFor(x => x.BreakMinutes)
            .GreaterThanOrEqualTo(0)
            .WithErrorCode(HrBusinessErrorCodes.ShiftTemplateInvalidBreakMinutes);
    }
}
