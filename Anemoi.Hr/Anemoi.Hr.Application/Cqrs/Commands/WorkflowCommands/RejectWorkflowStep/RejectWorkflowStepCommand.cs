using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.RejectWorkflowStep;

public sealed record RejectWorkflowStepCommand(
    string InstanceId,
    string? Comment,
    string? PerformedBy) : ICommandResult<WorkflowInstanceResponse>;
