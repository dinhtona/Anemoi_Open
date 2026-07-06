using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.WorkingCalendarRule.UpdateWorkingCalendarRule;

public sealed record UpdateWorkingCalendarRuleCommand(
    WorkingCalendarRuleId Id,
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
    bool WorkSunday) : ICommandResult<SuccessResponse>;
