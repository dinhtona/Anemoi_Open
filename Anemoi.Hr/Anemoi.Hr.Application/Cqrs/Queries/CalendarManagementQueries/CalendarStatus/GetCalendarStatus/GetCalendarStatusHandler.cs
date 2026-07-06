using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.CalendarManagement;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.CalendarStatus.GetCalendarStatus;

public sealed class GetCalendarStatusHandler(
    ISqlRepository<Domain.CalendarManagement.PublicHoliday> publicHolidayRepository,
    ISqlRepository<Domain.CalendarManagement.CompanyHoliday> companyHolidayRepository,
    ISqlRepository<Domain.CalendarManagement.CalendarException> calendarExceptionRepository,
    IWorkingCalendarEngine workingCalendarEngine)
    : IQueryHandler<GetCalendarStatusQuery, CalendarStatusResponse>
{
    public async Task<CalendarStatusResponse> Handle(
        GetCalendarStatusQuery request,
        CancellationToken cancellationToken)
    {
        var status = await workingCalendarEngine.GetCalendarStatus(request.Date, cancellationToken);

        var publicHoliday = await publicHolidayRepository.GetQueryable()
            .AsNoTracking()
            .Where(x => x.HolidayDate == request.Date ||
                (x.IsRecurringAnnual && x.HolidayDate.Day == request.Date.Day && x.HolidayDate.Month == request.Date.Month))
            .Select(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken);

        var companyHoliday = await companyHolidayRepository.GetQueryable()
            .AsNoTracking()
            .Where(x => x.HolidayDate == request.Date ||
                (x.IsRecurringAnnual && x.HolidayDate.Day == request.Date.Day && x.HolidayDate.Month == request.Date.Month))
            .Select(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken);

        var calendarException = await calendarExceptionRepository.GetQueryable()
            .AsNoTracking()
            .Where(x => x.ExceptionDate == request.Date)
            .Select(x => x.Reason)
            .FirstOrDefaultAsync(cancellationToken);

        return new CalendarStatusResponse
        {
            Date = request.Date,
            Status = status,
            HolidayName = companyHoliday ?? publicHoliday,
            ExceptionName = calendarException
        };
    }
}
