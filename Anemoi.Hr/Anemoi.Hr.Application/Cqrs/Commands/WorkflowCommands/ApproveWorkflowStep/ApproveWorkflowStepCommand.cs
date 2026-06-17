using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.ApproveWorkflowStep;

public sealed record ApproveWorkflowStepCommand(
    string InstanceId,
    string? Comment,
    string PerformedBy) : ICommandResult<WorkflowInstanceResponse>;
