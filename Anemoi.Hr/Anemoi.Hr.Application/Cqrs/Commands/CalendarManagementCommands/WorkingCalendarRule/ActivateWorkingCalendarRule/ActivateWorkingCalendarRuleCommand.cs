using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.WorkingCalendarRule.ActivateWorkingCalendarRule;

public sealed record ActivateWorkingCalendarRuleCommand(
    WorkingCalendarRuleId Id) : ICommandResult<SuccessResponse>;
