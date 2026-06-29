using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Workflow;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.CreateWorkflowDefinition;

public sealed class CreateWorkflowDefinitionValidator : AbstractValidator<CreateWorkflowDefinitionCommand>
{
    public CreateWorkflowDefinitionValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowCodeRequired);
        RuleFor(x => x.Name).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowNameRequired);
        RuleFor(x => x.WorkflowTypeCode).NotEmpty().WithErrorCode(HrBusinessErrorCodes.ValWorkflowApproverTypeRequired);
        RuleFor(x => x.Steps).NotEmpty().WithErrorCode(HrBusinessErrorCodes.WorkflowDefinitionNoSteps);
        RuleForEach(x => x.Steps).ChildRules(step =>
        {
            step.RuleFor(s => s.Sequence).GreaterThan(0).WithErrorCode(HrBusinessErrorCodes.ValWorkflowStepSequenceInvalid);
            step.RuleFor(s => s.ApproverType)
                .Must(t => t is ApproverType.Role or ApproverType.Permission
                    or ApproverType.DirectManager or ApproverType.DepartmentManager
                    or ApproverType.HrManager or ApproverType.SpecificUser)
                .WithErrorCode(HrBusinessErrorCodes.ValWorkflowApproverTypeRequired);
            step.When(s => s.ApproverType is ApproverType.SpecificUser or ApproverType.Role or ApproverType.Permission,
                () => step.RuleFor(s => s.ApproverValue)
                    .NotEmpty()
                    .WithErrorCode(HrBusinessErrorCodes.ValWorkflowApproverValueRequired));
        });
        RuleFor(x => x.Steps.Select(s => s.Sequence))
            .Must(seqs => seqs.Distinct().Count() == seqs.Count())
            .WithErrorCode(HrBusinessErrorCodes.ValWorkflowStepSequenceInvalid);
    }
}
