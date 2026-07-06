using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.WorkingCalendarRule.CreateWorkingCalendarRule;

public sealed record CreateWorkingCalendarRuleCommand(
    string Name,
    string? Description,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    bool WorkMonday,
    bool WorkTuesday,
    bool WorkWednesday,
    bool WorkThursday,
    bool WorkFriday,
    bool WorkSaturday,
    bool WorkSunday) : ICommandResult<WorkingCalendarRuleIdResponse>;
