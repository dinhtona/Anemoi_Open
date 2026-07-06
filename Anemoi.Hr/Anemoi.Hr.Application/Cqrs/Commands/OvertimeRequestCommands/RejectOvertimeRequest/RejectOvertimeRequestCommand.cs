using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.OvertimeRequestCommands.RejectOvertimeRequest;

[Obsolete("Use ApproveWorkflowStepCommand instead")]
public sealed record RejectOvertimeRequestCommand(
    OvertimeRequestId? Id,
    string RejectedBy,
    string Reason = null) : ICommandResult<SuccessResponse>;
