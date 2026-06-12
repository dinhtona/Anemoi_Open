namespace Anemoi.Hr.Application.Responses;

public sealed class CompanyHolidayResponse
{
    public Guid Id { get; init; }
    public DateOnly HolidayDate { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsRecurringAnnual { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
