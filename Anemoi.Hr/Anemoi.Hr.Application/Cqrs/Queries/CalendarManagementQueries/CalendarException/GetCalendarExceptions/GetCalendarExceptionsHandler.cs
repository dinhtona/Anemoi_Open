using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.CalendarManagement;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.CalendarException.GetCalendarExceptions;

public sealed class GetCalendarExceptionsHandler(
    ISqlRepository<Domain.CalendarManagement.CalendarException> calendarExceptionRepository,
    CalendarManagementMapper mapper)
    : IQueryHandler<GetCalendarExceptionsQuery, PaginationResponse<CalendarExceptionResponse>>
{
    public async Task<PaginationResponse<CalendarExceptionResponse>> Handle(
        GetCalendarExceptionsQuery request,
        CancellationToken cancellationToken)
    {
        var query = calendarExceptionRepository.GetQueryable().AsNoTracking();

        if (request.Year.HasValue)
            query = query.Where(x => x.ExceptionDate.Year == request.Year.Value);

        if (request.Month.HasValue)
            query = query.Where(x => x.ExceptionDate.Month == request.Month.Value);

        if (request.ExceptionType.HasValue)
            query = query.Where(x => x.ExceptionType == request.ExceptionType.Value);

        query = (request.SortBy?.ToLower(), request.SortDirection?.ToLower()) switch
        {
            ("exceptiondate", "desc") => query.OrderByDescending(x => x.ExceptionDate),
            ("exceptiondate", "asc") => query.OrderBy(x => x.ExceptionDate),
            ("createdat", "desc") => query.OrderByDescending(x => x.CreatedAt),
            ("createdat", "asc") => query.OrderBy(x => x.CreatedAt),
            _ => query.OrderBy(x => x.ExceptionDate)
        };

        var totalRecords = await query.LongCountAsync(cancellationToken);
        var rows = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginationResponse<CalendarExceptionResponse>(
            rows.Select(mapper.ToCalendarExceptionResponse).ToList(),
            totalRecords);
    }
}
