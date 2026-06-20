using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Workflow;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.UpdateWorkflowDefinition;

public sealed class UpdateWorkflowDefinitionValidator : AbstractValidator<UpdateWorkflowDefinitionCommand>
{
    public UpdateWorkflowDefinitionValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowDefinitionIdRequired);
        RuleFor(x => x.Name).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowNameRequired);
        RuleFor(x => x.Steps).NotEmpty().WithErrorCode(HrBusinessErrorCodes.WorkflowDefinitionNoSteps);
        RuleForEach(x => x.Steps).ChildRules(step =>
        {
            step.RuleFor(s => s.Sequence).GreaterThan(0).WithErrorCode(HrBusinessErrorCodes.ValWorkflowStepSequenceInvalid);
            step.RuleFor(s => s.ApproverType)
                .Must(t => t is ApproverType.Role or ApproverType.Permission
                    or ApproverType.DirectManager or ApproverType.DepartmentManager
                    or ApproverType.HrManager or ApproverType.SpecificUser)
                .WithErrorCode(HrBusinessErrorCodes.ValWorkflowApproverTypeRequired);
        });
    }
}
