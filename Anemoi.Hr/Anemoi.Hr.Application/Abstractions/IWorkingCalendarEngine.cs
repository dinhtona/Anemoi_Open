using Anemoi.Hr.Domain.CalendarManagement;

namespace Anemoi.Hr.Application.Abstractions;

public interface IWorkingCalendarEngine
{
    Task<bool> IsWorkingDay(DateOnly date, CancellationToken ct = default);
    Task<bool> IsHoliday(DateOnly date, CancellationToken ct = default);
    Task<CalendarStatus> GetCalendarStatus(DateOnly date, CancellationToken ct = default);
}
