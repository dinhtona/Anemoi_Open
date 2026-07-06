using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.AssignWorkflowRole;

public sealed class AssignWorkflowRoleValidator : AbstractValidator<AssignWorkflowRoleCommand>
{
    public AssignWorkflowRoleValidator()
    {
        RuleFor(x => x.Role)
            .NotEmpty()
            .WithErrorCode(HrBusinessErrorCodes.ValWorkflowRoleInvalid)
            .Must(r => r is WorkflowRole.HrManager)
            .WithErrorCode(HrBusinessErrorCodes.ValWorkflowRoleInvalid);

        RuleFor(x => x.EmployeeId)
            .RequiredId(HrBusinessErrorCodes.ValEmployeeIdRequired);
    }
}
