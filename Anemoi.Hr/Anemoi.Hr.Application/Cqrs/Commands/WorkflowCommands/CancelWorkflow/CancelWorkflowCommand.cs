using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.CancelWorkflow;

public sealed record CancelWorkflowCommand(
    string InstanceId,
    string? PerformedBy) : ICommandResult<WorkflowInstanceResponse>;
