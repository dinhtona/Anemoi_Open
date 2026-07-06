using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.CalendarException.GetCalendarExceptionById;

public sealed record GetCalendarExceptionByIdQuery(
    CalendarExceptionId Id) : IQuery<CalendarExceptionResponse>;
