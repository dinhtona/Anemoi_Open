using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.DeactivateWorkflowDefinition;

public sealed class DeactivateWorkflowDefinitionValidator : AbstractValidator<DeactivateWorkflowDefinitionCommand>
{
    public DeactivateWorkflowDefinitionValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowDefinitionIdRequired);
    }
}
