using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.ShiftManagementCommands.UpdateShiftTemplate;

public sealed record UpdateShiftTemplateCommand(
    ShiftTemplateId Id,
    string Code,
    string Name,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int BreakMinutes) : ICommandResult<SuccessResponse>;
