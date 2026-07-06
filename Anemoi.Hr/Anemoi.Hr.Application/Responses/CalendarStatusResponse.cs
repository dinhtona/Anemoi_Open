using Anemoi.Hr.Domain.CalendarManagement;

namespace Anemoi.Hr.Application.Responses;

public sealed class CalendarStatusResponse
{
    public DateOnly Date { get; init; }
    public CalendarStatus Status { get; init; }
    public string? HolidayName { get; init; }
    public string? ExceptionName { get; init; }
}
