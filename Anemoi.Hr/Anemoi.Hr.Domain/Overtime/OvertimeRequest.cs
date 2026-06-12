using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Overtime;

public sealed class OvertimeRequest : Entity<OvertimeRequestId>
{
    public EmployeeId EmployeeId { get; private set; }
    public DateOnly OvertimeDate { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public string Reason { get; private set; }
    public string Status { get; private set; }
    public string ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string RejectedBy { get; private set; }
    public DateTime? RejectedAt { get; private set; }
    public string CancelledBy { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Employee Employee { get; private set; }

    private OvertimeRequest() { }

    public static OvertimeRequest Create(
        EmployeeId employeeId,
        DateOnly overtimeDate,
        TimeOnly startTime,
        TimeOnly endTime,
        string reason,
        string createdBy = null)
    {
        ValidateCreation(employeeId, overtimeDate, startTime, endTime, reason);

        var overtimeRequest = new OvertimeRequest
        {
            Id = new OvertimeRequestId(Guid.NewGuid()),
            EmployeeId = employeeId,
            OvertimeDate = overtimeDate,
            StartTime = startTime,
            EndTime = endTime,
            Reason = reason,
            Status = OvertimeStatusCode.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return overtimeRequest;
    }

    public void Approve(string approvedBy)
    {
        EnsureValidTransition(OvertimeStatusCode.Approved);

        Status = OvertimeStatusCode.Approved;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(string rejectedBy, string reason = null)
    {
        EnsureValidTransition(OvertimeStatusCode.Rejected);

        Status = OvertimeStatusCode.Rejected;
        RejectedBy = rejectedBy;
        RejectedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string cancelledBy)
    {
        EnsureValidTransition(OvertimeStatusCode.Cancelled);

        Status = OvertimeStatusCode.Cancelled;
        CancelledBy = cancelledBy;
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal CalculateDurationHours()
    {
        var startMinutes = StartTime.Hour * 60 + StartTime.Minute;
        var endMinutes = EndTime.Hour * 60 + EndTime.Minute;
        var durationMinutes = endMinutes - startMinutes;
        return Math.Round((decimal)durationMinutes / 60m, 2);
    }

    public bool OverlapsWith(OvertimeRequest other)
    {
        if (OvertimeDate != other.OvertimeDate)
            return false;

        return StartTime < other.EndTime && EndTime > other.StartTime;
    }

    private void EnsureValidTransition(string targetStatus)
    {
        var currentStatus = Status;

        var allowed = (currentStatus, targetStatus) switch
        {
            (OvertimeStatusCode.Pending, OvertimeStatusCode.Approved) => true,
            (OvertimeStatusCode.Pending, OvertimeStatusCode.Rejected) => true,
            (OvertimeStatusCode.Pending, OvertimeStatusCode.Cancelled) => true,
            _ => false
        };

        if (!allowed)
            throw new InvalidOperationException(
                $"Cannot transition overtime request from '{currentStatus}' to '{targetStatus}'.");
    }

    private static void ValidateCreation(
        EmployeeId employeeId,
        DateOnly overtimeDate,
        TimeOnly startTime,
        TimeOnly endTime,
        string reason)
    {
        if (employeeId is null)
            throw new ArgumentException("Employee ID must not be null.", nameof(employeeId));

        if (startTime >= endTime)
            throw new ArgumentException("End time must be greater than start time.", nameof(endTime));

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason must not be null or whitespace.", nameof(reason));
    }
}
