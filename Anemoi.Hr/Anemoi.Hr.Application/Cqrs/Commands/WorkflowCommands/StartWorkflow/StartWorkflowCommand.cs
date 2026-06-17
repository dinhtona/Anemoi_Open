using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.StartWorkflow;

public sealed record StartWorkflowCommand(
    string DefinitionId,
    string EntityType,
    string EntityId,
    string StartedBy,
    string RequesterEmployeeId,
    string RequesterUserId) : ICommandResult<WorkflowInstanceResponse>;
