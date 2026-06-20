using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.AssignWorkflowRole;

public sealed record AssignWorkflowRoleCommand(
    string Role,
    EmployeeId EmployeeId) : ICommandResult<WorkflowRoleAssignmentResponse>;
