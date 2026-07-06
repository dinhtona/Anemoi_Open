using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.OvertimeRuleCommands.CreateOvertimeRule;

public sealed record CreateOvertimeRuleCommand(
    string Code,
    string Name,
    decimal WeekdayMultiplier,
    decimal WeekendMultiplier,
    decimal HolidayMultiplier) : ICommandResult<OvertimeRuleResponse>;
