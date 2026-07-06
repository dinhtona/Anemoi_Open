namespace Anemoi.Hr.Application.Responses;

public sealed class EmployeeScheduleCalendarResponse
{
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; }
    public string EmployeeName { get; init; }
    public List<CalendarDayResponse> Days { get; init; } = [];
}

public sealed class CalendarDayResponse
{
    public DateOnly Date { get; init; }
    public bool HasAssignment { get; init; }
    public Guid? AssignmentId { get; init; }
    public string ShiftName { get; init; }
    public TimeOnly? StartTime { get; init; }
    public TimeOnly? EndTime { get; init; }
    public decimal ExpectedWorkingHours { get; init; }
    public string Status { get; init; }
}
