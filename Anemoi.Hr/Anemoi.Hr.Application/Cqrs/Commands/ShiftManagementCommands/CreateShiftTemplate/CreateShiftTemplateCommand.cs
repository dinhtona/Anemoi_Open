using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.ShiftManagementCommands.CreateShiftTemplate;

public sealed record CreateShiftTemplateCommand(
    string Code,
    string Name,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int BreakMinutes) : ICommandResult<ShiftTemplateIdResponse>;
