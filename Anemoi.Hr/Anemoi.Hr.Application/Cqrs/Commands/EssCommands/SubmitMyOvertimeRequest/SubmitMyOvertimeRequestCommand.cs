using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.EssCommands.SubmitMyOvertimeRequest;

public sealed record SubmitMyOvertimeRequestCommand(
    string UserId,
    string Email,
    DateOnly OvertimeDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Reason) : ICommandResult<OvertimeRequestIdResponse>;
