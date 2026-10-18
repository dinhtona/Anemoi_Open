using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.CalendarManagement;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.PublicHoliday.GetPublicHolidayById;

public sealed class GetPublicHolidayByIdHandler(
    ISqlRepository<Domain.CalendarManagement.PublicHoliday> publicHolidayRepository,
    CalendarManagementMapper mapper)
    : IQueryHandler<GetPublicHolidayByIdQuery, OneOf<PublicHolidayResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<PublicHolidayResponse, ErrorDetailResponse>> Handle(
        GetPublicHolidayByIdQuery request,
        CancellationToken cancellationToken)
    {
        var publicHoliday = await publicHolidayRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        return publicHoliday is null ? null : mapper.ToPublicHolidayResponse(publicHoliday);
    }
}
