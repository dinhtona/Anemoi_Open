using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.CalendarStatus.GetCalendarStatus;

public sealed record GetCalendarStatusQuery(
    DateOnly Date) : IQuery<CalendarStatusResponse>;
