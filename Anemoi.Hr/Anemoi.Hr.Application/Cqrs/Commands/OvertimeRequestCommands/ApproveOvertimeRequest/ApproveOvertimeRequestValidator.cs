using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using Anemoi.Hr.ModelIds.ModelIds;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.OvertimeRequestCommands.ApproveOvertimeRequest;

public sealed class ApproveOvertimeRequestValidator : AbstractValidator<ApproveOvertimeRequestCommand>
{
    public ApproveOvertimeRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotNull()
            .WithErrorCode(HrBusinessErrorCodes.OvertimeRequestNotFound);

        RuleFor(x => x.ApprovedBy)
            .NotEmpty()
            .WithErrorCode(HrBusinessErrorCodes.OvertimeApproverRequired);
    }
}
