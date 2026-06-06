namespace Anemoi.Hr.Application.Responses;

public sealed class AttendanceSummaryResponse
{
    public decimal WorkedDays { get; set; }
    public decimal LeaveDays { get; set; }
    public decimal AbsentDays { get; set; }
    public decimal WorkedHours { get; set; }
}
