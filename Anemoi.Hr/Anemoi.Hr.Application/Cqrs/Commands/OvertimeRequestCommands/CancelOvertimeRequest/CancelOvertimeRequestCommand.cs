using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.OvertimeRequestCommands.CancelOvertimeRequest;

public sealed record CancelOvertimeRequestCommand(
    OvertimeRequestId Id,
    string CancelledBy) : ICommandResult<SuccessResponse>;
