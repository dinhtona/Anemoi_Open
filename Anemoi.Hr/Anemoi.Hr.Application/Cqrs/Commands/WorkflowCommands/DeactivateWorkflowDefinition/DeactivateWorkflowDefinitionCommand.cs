using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.DeactivateWorkflowDefinition;

public sealed record DeactivateWorkflowDefinitionCommand(string Id) : ICommandResult<WorkflowDefinitionResponse>;
