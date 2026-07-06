using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.CalendarException.GetCalendarExceptions;

public sealed record GetCalendarExceptionsQuery(
    int? Year = null,
    int? Month = null,
    Domain.CalendarManagement.CalendarStatus? ExceptionType = null,
    int Page = 1,
    int PageSize = 50,
    string SortBy = null,
    string SortDirection = "asc") : IQuery<PaginationResponse<CalendarExceptionResponse>>;
