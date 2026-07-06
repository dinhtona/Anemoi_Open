using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.ReturnWorkflow;

public sealed class ReturnWorkflowValidator : AbstractValidator<ReturnWorkflowCommand>
{
    public ReturnWorkflowValidator()
    {
        RuleFor(x => x.InstanceId).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowInstanceIdRequired);
    }
}
