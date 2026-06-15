using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.ShiftManagement;

public sealed class ShiftTemplate : Entity<ShiftTemplateId>
{
    public string Code { get; private set; }
    public string Name { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public int BreakMinutes { get; private set; }
    public decimal ExpectedWorkingHours { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private ShiftTemplate() { }

    public static ShiftTemplate Create(
        ShiftTemplateId id,
        string code,
        string name,
        TimeOnly startTime,
        TimeOnly endTime,
        int breakMinutes)
    {
        ValidateCreation(code, name, startTime, endTime, breakMinutes);

        var expectedHours = CalculateExpectedWorkingHours(startTime, endTime, breakMinutes);

        return new ShiftTemplate
        {
            Id = id,
            Code = code,
            Name = name,
            StartTime = startTime,
            EndTime = endTime,
            BreakMinutes = breakMinutes,
            ExpectedWorkingHours = expectedHours,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string code,
        string name,
        TimeOnly startTime,
        TimeOnly endTime,
        int breakMinutes)
    {
        ValidateCreation(code, name, startTime, endTime, breakMinutes);

        Code = code;
        Name = name;
        StartTime = startTime;
        EndTime = endTime;
        BreakMinutes = breakMinutes;
        ExpectedWorkingHours = CalculateExpectedWorkingHours(startTime, endTime, breakMinutes);
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

    public static decimal CalculateExpectedWorkingHours(TimeOnly startTime, TimeOnly endTime, int breakMinutes)
    {
        var startMinutes = (int)startTime.ToTimeSpan().TotalMinutes;
        var endMinutes = (int)endTime.ToTimeSpan().TotalMinutes;
        var durationMinutes = endMinutes > startMinutes
            ? endMinutes - startMinutes
            : 24 * 60 - startMinutes + endMinutes;
        var totalMinutes = durationMinutes - breakMinutes;
        if (totalMinutes < 0) totalMinutes = 0;
        return Math.Round((decimal)totalMinutes / 60m, 2);
    }

    private static void ValidateCreation(
        string code,
        string name,
        TimeOnly startTime,
        TimeOnly endTime,
        int breakMinutes)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code must not be null or whitespace.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name must not be null or whitespace.", nameof(name));
        if (startTime == endTime)
            throw new ArgumentException("Start time and end time must not be equal.", nameof(endTime));
        if (breakMinutes < 0)
            throw new ArgumentException("Break minutes must be greater than or equal to 0.", nameof(breakMinutes));
    }
}
