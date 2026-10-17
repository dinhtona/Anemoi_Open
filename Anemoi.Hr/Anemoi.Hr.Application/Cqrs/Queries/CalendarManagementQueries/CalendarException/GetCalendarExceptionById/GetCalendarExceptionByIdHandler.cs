using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.CalendarManagement;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.CalendarException.GetCalendarExceptionById;

public sealed class GetCalendarExceptionByIdHandler(
    ISqlRepository<Domain.CalendarManagement.CalendarException> calendarExceptionRepository,
    CalendarManagementMapper mapper)
    : IQueryHandler<GetCalendarExceptionByIdQuery, OneOf<CalendarExceptionResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<CalendarExceptionResponse, ErrorDetailResponse>> Handle(
        GetCalendarExceptionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var calendarException = await calendarExceptionRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        return calendarException is null ? null : mapper.ToCalendarExceptionResponse(calendarException);
    }
}
