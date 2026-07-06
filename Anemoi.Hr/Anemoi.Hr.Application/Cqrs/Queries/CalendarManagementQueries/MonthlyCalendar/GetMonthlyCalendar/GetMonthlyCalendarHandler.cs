using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.CalendarManagement;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.MonthlyCalendar.GetMonthlyCalendar;

public sealed class GetMonthlyCalendarHandler(
    ISqlRepository<Domain.CalendarManagement.PublicHoliday> publicHolidayRepository,
    ISqlRepository<Domain.CalendarManagement.CompanyHoliday> companyHolidayRepository,
    ISqlRepository<Domain.CalendarManagement.CalendarException> calendarExceptionRepository,
    IWorkingCalendarEngine workingCalendarEngine)
    : IQueryHandler<GetMonthlyCalendarQuery, MonthlyCalendarResponse>
{
    public async Task<MonthlyCalendarResponse> Handle(
        GetMonthlyCalendarQuery request,
        CancellationToken cancellationToken)
    {
        var daysInMonth = DateTime.DaysInMonth(request.Year, request.Month);
        var startDate = new DateOnly(request.Year, request.Month, 1);
        var endDate = new DateOnly(request.Year, request.Month, daysInMonth);

        var publicHolidays = await publicHolidayRepository.GetQueryable()
            .AsNoTracking()
            .Where(x => (x.HolidayDate >= startDate && x.HolidayDate <= endDate) ||
                        (x.IsRecurringAnnual && x.HolidayDate.Month == request.Month))
            .ToListAsync(cancellationToken);

        var companyHolidays = await companyHolidayRepository.GetQueryable()
            .AsNoTracking()
            .Where(x => (x.HolidayDate >= startDate && x.HolidayDate <= endDate) ||
                        (x.IsRecurringAnnual && x.HolidayDate.Month == request.Month))
            .ToListAsync(cancellationToken);

        var calendarExceptions = await calendarExceptionRepository.GetQueryable()
            .AsNoTracking()
            .Where(x => x.ExceptionDate >= startDate && x.ExceptionDate <= endDate)
            .ToListAsync(cancellationToken);

        var days = new List<DailyCalendarStatus>();
        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateOnly(request.Year, request.Month, day);
            var status = await workingCalendarEngine.GetCalendarStatus(date, cancellationToken);

            var holidayName = companyHolidays
                .Where(h => h.HolidayDate == date || (h.IsRecurringAnnual && h.HolidayDate.Day == date.Day && h.HolidayDate.Month == date.Month))
                .Select(h => h.Name)
                .FirstOrDefault() ??
                publicHolidays
                .Where(h => h.HolidayDate == date || (h.IsRecurringAnnual && h.HolidayDate.Day == date.Day && h.HolidayDate.Month == date.Month))
                .Select(h => h.Name)
                .FirstOrDefault();

            var exceptionName = calendarExceptions
                .Where(e => e.ExceptionDate == date)
                .Select(e => e.Reason)
                .FirstOrDefault();

            days.Add(new DailyCalendarStatus
            {
                Date = date,
                DayOfWeek = date.DayOfWeek,
                Status = status,
                HolidayName = holidayName,
                ExceptionName = exceptionName
            });
        }

        return new MonthlyCalendarResponse
        {
            Year = request.Year,
            Month = request.Month,
            Days = days
        };
    }
}
