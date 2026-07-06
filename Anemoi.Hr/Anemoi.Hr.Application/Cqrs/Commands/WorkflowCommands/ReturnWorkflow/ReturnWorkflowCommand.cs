using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.ReturnWorkflow;

public sealed record ReturnWorkflowCommand(
    string InstanceId,
    string? Comment,
    string? PerformedBy) : ICommandResult<WorkflowInstanceResponse>;
