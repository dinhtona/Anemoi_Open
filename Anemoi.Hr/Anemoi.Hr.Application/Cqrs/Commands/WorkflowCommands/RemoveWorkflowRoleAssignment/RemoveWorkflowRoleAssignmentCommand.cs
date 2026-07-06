using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.RemoveWorkflowRoleAssignment;

public sealed record RemoveWorkflowRoleAssignmentCommand(
    WorkflowRoleAssignmentId Id) : ICommandResult<SuccessResponse>;
