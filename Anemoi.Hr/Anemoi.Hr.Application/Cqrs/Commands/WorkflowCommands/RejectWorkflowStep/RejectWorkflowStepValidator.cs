using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.RejectWorkflowStep;

public sealed class RejectWorkflowStepValidator : AbstractValidator<RejectWorkflowStepCommand>
{
    public RejectWorkflowStepValidator()
    {
        RuleFor(x => x.InstanceId).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowInstanceIdRequired);
    }
}
