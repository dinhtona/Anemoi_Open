using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.PositionCommands.UpdatePosition;

public sealed record UpdatePositionCommand(
    PositionId Id,
    string Code,
    string Name,
    string PositionTypeCode,
    DepartmentId DepartmentId) : ICommandResult<PositionResponse>;
