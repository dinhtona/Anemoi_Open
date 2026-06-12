using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.CalendarManagement;

public sealed class CompanyHoliday : Entity<CompanyHolidayId>
{
    public DateOnly HolidayDate { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsRecurringAnnual { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private CompanyHoliday() { }

    public static CompanyHoliday Create(
        DateOnly holidayDate,
        string name,
        string? description,
        bool isRecurringAnnual)
    {
        ValidateCreation(holidayDate, name);

        return new CompanyHoliday
        {
            Id = new CompanyHolidayId(IdGenerator.NextGuid()),
            HolidayDate = holidayDate,
            Name = name,
            Description = description,
            IsRecurringAnnual = isRecurringAnnual,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        DateOnly holidayDate,
        string name,
        string? description,
        bool isRecurringAnnual)
    {
        ValidateCreation(holidayDate, name);

        HolidayDate = holidayDate;
        Name = name;
        Description = description;
        IsRecurringAnnual = isRecurringAnnual;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateCreation(DateOnly holidayDate, string name)
    {
        if (holidayDate == DateOnly.MinValue)
            throw new ArgumentException("Holiday date is required.", nameof(holidayDate));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name must not be null or whitespace.", nameof(name));
    }
}
