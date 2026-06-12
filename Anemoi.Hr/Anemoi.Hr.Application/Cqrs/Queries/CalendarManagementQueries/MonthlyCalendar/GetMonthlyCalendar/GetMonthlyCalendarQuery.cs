using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.MonthlyCalendar.GetMonthlyCalendar;

public sealed record GetMonthlyCalendarQuery(
    int Year,
    int Month) : IQuery<MonthlyCalendarResponse>;
