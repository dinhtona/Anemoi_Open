using Anemoi.Hr.Domain.CalendarManagement;

namespace Anemoi.Hr.Application.Responses;

public sealed class DailyCalendarStatus
{
    public DateOnly Date { get; init; }
    public DayOfWeek DayOfWeek { get; init; }
    public CalendarStatus Status { get; init; }
    public string? HolidayName { get; init; }
    public string? ExceptionName { get; init; }
}

public sealed class MonthlyCalendarResponse
{
    public int Year { get; init; }
    public int Month { get; init; }
    public List<DailyCalendarStatus> Days { get; init; } = [];
}
