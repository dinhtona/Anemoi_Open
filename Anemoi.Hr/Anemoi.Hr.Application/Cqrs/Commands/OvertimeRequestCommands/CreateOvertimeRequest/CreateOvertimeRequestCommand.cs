using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.OvertimeRequestCommands.CreateOvertimeRequest;

public sealed record CreateOvertimeRequestCommand(
    EmployeeId EmployeeId,
    DateOnly OvertimeDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Reason) : ICommandResult<OvertimeRequestIdResponse>;
