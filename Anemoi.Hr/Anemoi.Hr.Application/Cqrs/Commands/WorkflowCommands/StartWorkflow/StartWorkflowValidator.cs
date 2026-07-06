using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.StartWorkflow;

public sealed class StartWorkflowValidator : AbstractValidator<StartWorkflowCommand>
{
    public StartWorkflowValidator()
    {
        RuleFor(x => x.DefinitionId).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowDefinitionIdRequired);
        RuleFor(x => x.EntityType).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowApproverTypeRequired);
        RuleFor(x => x.EntityId).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowInstanceIdRequired);
    }
}
