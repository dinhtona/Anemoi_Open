namespace Anemoi.Hr.Application.Responses;

public sealed class WorkingCalendarRuleResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateOnly EffectiveFrom { get; init; }
    public DateOnly? EffectiveTo { get; init; }
    public bool WorkMonday { get; init; }
    public bool WorkTuesday { get; init; }
    public bool WorkWednesday { get; init; }
    public bool WorkThursday { get; init; }
    public bool WorkFriday { get; init; }
    public bool WorkSaturday { get; init; }
    public bool WorkSunday { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
