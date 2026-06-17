using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.ActivateWorkflowDefinition;

public sealed record ActivateWorkflowDefinitionCommand(string Id) : ICommandResult<WorkflowDefinitionResponse>;
