using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.CancelWorkflow;

public sealed class CancelWorkflowValidator : AbstractValidator<CancelWorkflowCommand>
{
    public CancelWorkflowValidator()
    {
        RuleFor(x => x.InstanceId).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowInstanceIdRequired);
    }
}
