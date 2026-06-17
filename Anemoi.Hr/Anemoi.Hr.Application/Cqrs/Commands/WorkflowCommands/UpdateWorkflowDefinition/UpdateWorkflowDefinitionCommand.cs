using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.Common;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.UpdateWorkflowDefinition;

public sealed record UpdateWorkflowDefinitionCommand(
    string Id,
    string Name,
    string? Description,
    List<WorkflowDefinitionStepInput> Steps) : ICommandResult<WorkflowDefinitionResponse>;
