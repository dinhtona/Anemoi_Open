namespace Anemoi.Hr.Application.Responses;

public sealed class OvertimeRequestResponse
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; }
    public string EmployeeName { get; init; }
    public DateOnly OvertimeDate { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public decimal DurationHours { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ApprovedBy { get; init; }
    public DateTime? ApprovedAt { get; init; }
    public string RejectedBy { get; init; }
    public DateTime? RejectedAt { get; init; }
    public string CancelledBy { get; init; }
    public DateTime? CancelledAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CurrentApproverName { get; set; }
    public string? CurrentStepName { get; set; }
    public string? WorkflowStatus { get; set; }
}
