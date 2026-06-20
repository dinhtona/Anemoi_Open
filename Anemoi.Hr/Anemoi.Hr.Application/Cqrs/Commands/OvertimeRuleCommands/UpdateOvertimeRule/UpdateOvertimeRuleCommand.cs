using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.OvertimeRuleCommands.UpdateOvertimeRule;

public sealed record UpdateOvertimeRuleCommand(
    OvertimeRuleId Id,
    string Code,
    string Name,
    decimal WeekdayMultiplier,
    decimal WeekendMultiplier,
    decimal HolidayMultiplier) : ICommandResult<OvertimeRuleResponse>;
