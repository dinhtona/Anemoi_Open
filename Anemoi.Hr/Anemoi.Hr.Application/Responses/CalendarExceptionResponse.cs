using Anemoi.Hr.Domain.CalendarManagement;

namespace Anemoi.Hr.Application.Responses;

public sealed class CalendarExceptionResponse
{
    public Guid Id { get; init; }
    public DateOnly ExceptionDate { get; init; }
    public string ExceptionType { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid? RelatedHolidayId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
