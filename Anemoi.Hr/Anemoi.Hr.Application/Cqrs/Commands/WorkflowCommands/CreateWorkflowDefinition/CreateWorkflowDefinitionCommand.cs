using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.Common;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.CreateWorkflowDefinition;

public sealed record CreateWorkflowDefinitionCommand(
    string Code,
    string Name,
    string? Description,
    string WorkflowTypeCode,
    List<WorkflowDefinitionStepInput> Steps) : ICommandResult<WorkflowDefinitionResponse>;
