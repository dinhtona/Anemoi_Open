namespace Anemoi.Hr.Application.Responses;

public sealed class EmployeeShiftAssignmentResponse
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; }
    public string EmployeeName { get; init; }
    public Guid ShiftTemplateId { get; init; }
    public string ShiftTemplateCode { get; init; }
    public string ShiftTemplateName { get; init; }
    public DateOnly WorkDate { get; init; }
    public string ShiftNameSnapshot { get; init; } = string.Empty;
    public TimeOnly StartTimeSnapshot { get; init; }
    public TimeOnly EndTimeSnapshot { get; init; }
    public int BreakMinutesSnapshot { get; init; }
    public decimal ExpectedWorkingHoursSnapshot { get; init; }
    public string Status { get; init; } = string.Empty;
    public string AssignedBy { get; init; } = string.Empty;
    public DateTime AssignedAt { get; init; }
    public string CancelledBy { get; init; }
    public DateTime? CancelledAt { get; init; }
    public string CancellationReason { get; init; }
}
