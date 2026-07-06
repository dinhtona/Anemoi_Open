using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Overtime;

public sealed class OvertimeRule : Entity<OvertimeRuleId>
{
    public string Code { get; private set; }
    public string Name { get; private set; }
    public decimal WeekdayMultiplier { get; private set; }
    public decimal WeekendMultiplier { get; private set; }
    public decimal HolidayMultiplier { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private OvertimeRule() { }

    public static OvertimeRule Create(
        OvertimeRuleId id,
        string code,
        string name,
        decimal weekdayMultiplier,
        decimal weekendMultiplier,
        decimal holidayMultiplier)
    {
        return new OvertimeRule
        {
            Id = id,
            Code = code,
            Name = name,
            WeekdayMultiplier = weekdayMultiplier,
            WeekendMultiplier = weekendMultiplier,
            HolidayMultiplier = holidayMultiplier,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateInfo(
        string code,
        string name,
        decimal weekdayMultiplier,
        decimal weekendMultiplier,
        decimal holidayMultiplier)
    {
        Code = code;
        Name = name;
        WeekdayMultiplier = weekdayMultiplier;
        WeekendMultiplier = weekendMultiplier;
        HolidayMultiplier = holidayMultiplier;
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
