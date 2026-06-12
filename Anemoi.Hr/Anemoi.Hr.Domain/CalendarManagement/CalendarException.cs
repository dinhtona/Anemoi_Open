using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.CalendarManagement;

public sealed class CalendarException : Entity<CalendarExceptionId>
{
    public DateOnly ExceptionDate { get; private set; }
    public CalendarStatus ExceptionType { get; private set; }
    public string Reason { get; private set; }
    public PublicHolidayId? RelatedHolidayId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private CalendarException() { }

    public static CalendarException Create(
        DateOnly exceptionDate,
        CalendarStatus exceptionType,
        string reason,
        PublicHolidayId? relatedHolidayId)
    {
        ValidateCreation(exceptionDate, reason);

        return new CalendarException
        {
            Id = new CalendarExceptionId(IdGenerator.NextGuid()),
            ExceptionDate = exceptionDate,
            ExceptionType = exceptionType,
            Reason = reason,
            RelatedHolidayId = relatedHolidayId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        DateOnly exceptionDate,
        CalendarStatus exceptionType,
        string reason,
        PublicHolidayId? relatedHolidayId)
    {
        ValidateCreation(exceptionDate, reason);

        ExceptionDate = exceptionDate;
        ExceptionType = exceptionType;
        Reason = reason;
        RelatedHolidayId = relatedHolidayId;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateCreation(DateOnly exceptionDate, string reason)
    {
        if (exceptionDate == DateOnly.MinValue)
            throw new ArgumentException("Exception date is required.", nameof(exceptionDate));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason must not be null or whitespace.", nameof(reason));
    }
}
