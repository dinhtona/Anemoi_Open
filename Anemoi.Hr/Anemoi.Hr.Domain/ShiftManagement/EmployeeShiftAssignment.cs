using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.ShiftManagement;

public sealed class EmployeeShiftAssignment : Entity<EmployeeShiftAssignmentId>
{
    public EmployeeId EmployeeId { get; private set; }
    public ShiftTemplateId ShiftTemplateId { get; private set; }
    public DateOnly WorkDate { get; private set; }
    public string ShiftNameSnapshot { get; private set; }
    public TimeOnly StartTimeSnapshot { get; private set; }
    public TimeOnly EndTimeSnapshot { get; private set; }
    public int BreakMinutesSnapshot { get; private set; }
    public decimal ExpectedWorkingHoursSnapshot { get; private set; }
    public string Status { get; private set; }
    public string AssignedBy { get; private set; }
    public DateTime AssignedAt { get; private set; }
    public string CancelledBy { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string CancellationReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Employee Employee { get; private set; }
    public ShiftTemplate ShiftTemplate { get; private set; }

    private EmployeeShiftAssignment() { }

    public static EmployeeShiftAssignment Create(
        EmployeeId employeeId,
        ShiftTemplate shiftTemplate,
        DateOnly workDate,
        string assignedBy)
    {
        if (employeeId is null)
            throw new ArgumentException("Employee ID must not be null.", nameof(employeeId));
        if (shiftTemplate is null)
            throw new ArgumentException("Shift template must not be null.", nameof(shiftTemplate));
        if (!shiftTemplate.IsActive)
            throw new ArgumentException("Cannot assign an inactive shift template.", nameof(shiftTemplate));
        if (string.IsNullOrWhiteSpace(assignedBy))
            throw new ArgumentException("Assigned by must not be null or whitespace.", nameof(assignedBy));

        return new EmployeeShiftAssignment
        {
            Id = new EmployeeShiftAssignmentId(IdGenerator.NextGuid()),
            EmployeeId = employeeId,
            ShiftTemplateId = shiftTemplate.Id,
            WorkDate = workDate,
            ShiftNameSnapshot = shiftTemplate.Name,
            StartTimeSnapshot = shiftTemplate.StartTime,
            EndTimeSnapshot = shiftTemplate.EndTime,
            BreakMinutesSnapshot = shiftTemplate.BreakMinutes,
            ExpectedWorkingHoursSnapshot = shiftTemplate.ExpectedWorkingHours,
            Status = EmployeeShiftAssignmentStatusCode.Assigned,
            AssignedBy = assignedBy,
            AssignedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Cancel(string cancelledBy, string cancellationReason = null)
    {
        if (Status != EmployeeShiftAssignmentStatusCode.Assigned)
            throw new InvalidOperationException(
                $"Cannot cancel assignment in '{Status}' status.");

        Status = EmployeeShiftAssignmentStatusCode.Cancelled;
        CancelledBy = cancelledBy;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = cancellationReason;
        UpdatedAt = DateTime.UtcNow;
    }
}
