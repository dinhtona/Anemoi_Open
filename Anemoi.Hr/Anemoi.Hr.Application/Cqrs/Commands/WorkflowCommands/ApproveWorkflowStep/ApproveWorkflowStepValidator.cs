using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.ApproveWorkflowStep;

public sealed class ApproveWorkflowStepValidator : AbstractValidator<ApproveWorkflowStepCommand>
{
    public ApproveWorkflowStepValidator()
    {
        RuleFor(x => x.InstanceId).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowInstanceIdRequired);
    }
}
