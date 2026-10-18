using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.CalendarManagement;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.CompanyHoliday.GetCompanyHolidayById;

public sealed class GetCompanyHolidayByIdHandler(
    ISqlRepository<Domain.CalendarManagement.CompanyHoliday> companyHolidayRepository,
    CalendarManagementMapper mapper)
    : IQueryHandler<GetCompanyHolidayByIdQuery, OneOf<CompanyHolidayResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<CompanyHolidayResponse, ErrorDetailResponse>> Handle(
        GetCompanyHolidayByIdQuery request,
        CancellationToken cancellationToken)
    {
        var companyHoliday = await companyHolidayRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        return companyHoliday is null ? null : mapper.ToCompanyHolidayResponse(companyHoliday);
    }
}
