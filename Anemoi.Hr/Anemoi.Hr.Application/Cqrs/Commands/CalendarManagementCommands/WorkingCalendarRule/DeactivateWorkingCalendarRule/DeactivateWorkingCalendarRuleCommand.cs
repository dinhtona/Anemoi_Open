using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.WorkingCalendarRule.DeactivateWorkingCalendarRule;

public sealed record DeactivateWorkingCalendarRuleCommand(
    WorkingCalendarRuleId Id) : ICommandResult<SuccessResponse>;
