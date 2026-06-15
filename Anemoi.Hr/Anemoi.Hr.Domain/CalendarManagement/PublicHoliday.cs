using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.CalendarManagement;

public sealed class PublicHoliday : Entity<PublicHolidayId>
{
    public DateOnly HolidayDate { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string CountryCode { get; private set; }
    public bool IsRecurringAnnual { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private PublicHoliday() { }

    public static PublicHoliday Create(
        PublicHolidayId id,
        DateOnly holidayDate,
        string name,
        string? description,
        string countryCode,
        bool isRecurringAnnual)
    {
        ValidateCreation(holidayDate, name, countryCode);

        return new PublicHoliday
        {
            Id = id,
            HolidayDate = holidayDate,
            Name = name,
            Description = description,
            CountryCode = countryCode,
            IsRecurringAnnual = isRecurringAnnual,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        DateOnly holidayDate,
        string name,
        string? description,
        string countryCode,
        bool isRecurringAnnual)
    {
        ValidateCreation(holidayDate, name, countryCode);

        HolidayDate = holidayDate;
        Name = name;
        Description = description;
        CountryCode = countryCode;
        IsRecurringAnnual = isRecurringAnnual;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateCreation(DateOnly holidayDate, string name, string countryCode)
    {
        if (holidayDate == DateOnly.MinValue)
            throw new ArgumentException("Holiday date is required.", nameof(holidayDate));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name must not be null or whitespace.", nameof(name));
        if (string.IsNullOrWhiteSpace(countryCode))
            throw new ArgumentException("Country code must not be null or whitespace.", nameof(countryCode));
    }
}
