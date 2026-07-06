using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.EssCommands.CancelMyOvertimeRequest;

public sealed record CancelMyOvertimeRequestCommand(
    string UserId,
    string Email,
    OvertimeRequestId OvertimeRequestId) : ICommandResult<SuccessResponse>;
