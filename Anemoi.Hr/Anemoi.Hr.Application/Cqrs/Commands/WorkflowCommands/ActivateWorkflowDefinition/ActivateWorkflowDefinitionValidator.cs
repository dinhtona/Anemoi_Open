using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.ActivateWorkflowDefinition;

public sealed class ActivateWorkflowDefinitionValidator : AbstractValidator<ActivateWorkflowDefinitionCommand>
{
    public ActivateWorkflowDefinitionValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowDefinitionIdRequired);
    }
}
