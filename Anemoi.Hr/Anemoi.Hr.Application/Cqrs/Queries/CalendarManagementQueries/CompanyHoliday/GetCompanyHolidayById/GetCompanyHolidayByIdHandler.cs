using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.CalendarManagement;

namespace Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.CompanyHoliday.GetCompanyHolidayById;

public sealed class GetCompanyHolidayByIdHandler(
    ISqlRepository<Domain.CalendarManagement.CompanyHoliday> companyHolidayRepository,
    CalendarManagementMapper mapper)
    : IQueryHandler<GetCompanyHolidayByIdQuery, CompanyHolidayResponse>
{
    public async Task<CompanyHolidayResponse> Handle(
        GetCompanyHolidayByIdQuery request,
        CancellationToken cancellationToken)
    {
        var companyHoliday = await companyHolidayRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        return companyHoliday is null ? null : mapper.ToCompanyHolidayResponse(companyHoliday);
    }
}
