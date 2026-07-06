namespace Anemoi.Hr.Application.Responses;

public sealed class PublicHolidayResponse
{
    public Guid Id { get; init; }
    public DateOnly HolidayDate { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string CountryCode { get; init; } = string.Empty;
    public bool IsRecurringAnnual { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
