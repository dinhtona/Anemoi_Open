using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.CalendarManagement;

namespace Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.CalendarException.GetCalendarExceptionById;

public sealed class GetCalendarExceptionByIdHandler(
    ISqlRepository<Domain.CalendarManagement.CalendarException> calendarExceptionRepository,
    CalendarManagementMapper mapper)
    : IQueryHandler<GetCalendarExceptionByIdQuery, CalendarExceptionResponse>
{
    public async Task<CalendarExceptionResponse> Handle(
        GetCalendarExceptionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var calendarException = await calendarExceptionRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        return calendarException is null ? null : mapper.ToCalendarExceptionResponse(calendarException);
    }
}
