using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.OvertimeRequestCommands.ApproveOvertimeRequest;

[Obsolete("Use ApproveWorkflowStepCommand instead")]
public sealed record ApproveOvertimeRequestCommand(
    OvertimeRequestId? Id,
    string ApprovedBy) : ICommandResult<SuccessResponse>;
