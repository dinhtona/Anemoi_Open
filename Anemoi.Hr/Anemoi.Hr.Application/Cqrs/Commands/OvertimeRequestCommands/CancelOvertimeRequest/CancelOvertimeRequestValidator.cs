using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using Anemoi.Hr.ModelIds.ModelIds;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.OvertimeRequestCommands.CancelOvertimeRequest;

public sealed class CancelOvertimeRequestValidator : AbstractValidator<CancelOvertimeRequestCommand>
{
    public CancelOvertimeRequestValidator()
    {
        RuleFor(x => x.CancelledBy)
            .NotEmpty()
            .WithErrorCode(HrBusinessErrorCodes.OvertimeApproverRequired);
    }
}
