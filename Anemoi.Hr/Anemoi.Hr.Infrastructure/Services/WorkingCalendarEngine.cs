using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Domain.CalendarManagement;
using Anemoi.Hr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Infrastructure.Services;

public sealed class WorkingCalendarEngine(HrDbContext dbContext) : IWorkingCalendarEngine
{
    private static readonly Dictionary<string, DayOfWeek> DayAbbreviations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Mon"] = DayOfWeek.Monday,
        ["Tue"] = DayOfWeek.Tuesday,
        ["Wed"] = DayOfWeek.Wednesday,
        ["Thu"] = DayOfWeek.Thursday,
        ["Fri"] = DayOfWeek.Friday,
        ["Sat"] = DayOfWeek.Saturday,
        ["Sun"] = DayOfWeek.Sunday
    };

    public async Task<bool> IsWorkingDay(DateOnly date, CancellationToken ct = default)
    {
        var status = await GetCalendarStatus(date, ct);
        return status == CalendarStatus.WorkingDay;
    }

    public async Task<bool> IsHoliday(DateOnly date, CancellationToken ct = default)
    {
        var status = await GetCalendarStatus(date, ct);
        return status == CalendarStatus.Holiday;
    }

    public async Task<CalendarStatus> GetCalendarStatus(DateOnly date, CancellationToken ct = default)
    {
        var exception = await dbContext.Set<CalendarException>()
            .FirstOrDefaultAsync(x => x.ExceptionDate == date, ct);
        if (exception != null)
            return exception.ExceptionType == CalendarStatus.WorkingDay
                ? CalendarStatus.WorkingDay
                : CalendarStatus.Holiday;

        var companyHoliday = await dbContext.Set<CompanyHoliday>()
            .AnyAsync(x => x.HolidayDate == date ||
                (x.IsRecurringAnnual && x.HolidayDate.Day == date.Day && x.HolidayDate.Month == date.Month), ct);
        if (companyHoliday) return CalendarStatus.Holiday;

        var publicHoliday = await dbContext.Set<PublicHoliday>()
            .AnyAsync(x => x.HolidayDate == date ||
                (x.IsRecurringAnnual && x.HolidayDate.Day == date.Day && x.HolidayDate.Month == date.Month), ct);
        if (publicHoliday) return CalendarStatus.Holiday;

        var activeRule = await dbContext.Set<WorkingCalendarRule>()
            .AsNoTracking()
            .Where(x => x.IsActive && x.EffectiveFrom <= date && (x.EffectiveTo == null || x.EffectiveTo >= date))
            .OrderByDescending(x => x.EffectiveFrom)
            .FirstOrDefaultAsync(ct);

        if (activeRule == null) return CalendarStatus.NotSet;

        var isWorkingDay = date.DayOfWeek switch
        {
            DayOfWeek.Monday => activeRule.WorkMonday,
            DayOfWeek.Tuesday => activeRule.WorkTuesday,
            DayOfWeek.Wednesday => activeRule.WorkWednesday,
            DayOfWeek.Thursday => activeRule.WorkThursday,
            DayOfWeek.Friday => activeRule.WorkFriday,
            DayOfWeek.Saturday => activeRule.WorkSaturday,
            DayOfWeek.Sunday => activeRule.WorkSunday,
            _ => false
        };

        return isWorkingDay
            ? CalendarStatus.WorkingDay
            : CalendarStatus.Holiday;
    }
}
