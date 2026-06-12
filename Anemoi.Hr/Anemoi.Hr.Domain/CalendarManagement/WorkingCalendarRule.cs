using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.CalendarManagement;

public sealed class WorkingCalendarRule : Entity<WorkingCalendarRuleId>
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool WorkMonday { get; private set; }
    public bool WorkTuesday { get; private set; }
    public bool WorkWednesday { get; private set; }
    public bool WorkThursday { get; private set; }
    public bool WorkFriday { get; private set; }
    public bool WorkSaturday { get; private set; }
    public bool WorkSunday { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private WorkingCalendarRule() { }

    public static WorkingCalendarRule Create(
        string name,
        string? description,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        bool workMonday,
        bool workTuesday,
        bool workWednesday,
        bool workThursday,
        bool workFriday,
        bool workSaturday,
        bool workSunday)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name must not be null or whitespace.", nameof(name));
        if (effectiveFrom == DateOnly.MinValue)
            throw new ArgumentException("Effective from date is required.", nameof(effectiveFrom));

        return new WorkingCalendarRule
        {
            Id = new WorkingCalendarRuleId(IdGenerator.NextGuid()),
            Name = name,
            Description = description,
            WorkMonday = workMonday,
            WorkTuesday = workTuesday,
            WorkWednesday = workWednesday,
            WorkThursday = workThursday,
            WorkFriday = workFriday,
            WorkSaturday = workSaturday,
            WorkSunday = workSunday,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name,
        string? description,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        bool workMonday,
        bool workTuesday,
        bool workWednesday,
        bool workThursday,
        bool workFriday,
        bool workSaturday,
        bool workSunday)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name must not be null or whitespace.", nameof(name));
        if (effectiveFrom == DateOnly.MinValue)
            throw new ArgumentException("Effective from date is required.", nameof(effectiveFrom));

        Name = name;
        Description = description;
        WorkMonday = workMonday;
        WorkTuesday = workTuesday;
        WorkWednesday = workWednesday;
        WorkThursday = workThursday;
        WorkFriday = workFriday;
        WorkSaturday = workSaturday;
        WorkSunday = workSunday;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
