using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.WorkingCalendarRule.GetWorkingCalendarRuleById;

public sealed record GetWorkingCalendarRuleByIdQuery(
    WorkingCalendarRuleId Id) : IQuery<WorkingCalendarRuleResponse>;
