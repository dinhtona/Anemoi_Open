using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Attendance;

public sealed class AttendanceRecord : ValueObject
{
    public AttendanceRecordId Id { get; set; }
    public AttendancePeriodId AttendancePeriodId { get; set; }
    public EmployeeId EmployeeId { get; set; }
    public DateOnly WorkDate { get; set; }
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    public decimal WorkedHours { get; set; }
    public decimal WorkedDays { get; set; }
    public string Status { get; set; }
    public LeaveRequestId? LeaveRequestId { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }

    // Navigation properties
    public AttendancePeriod AttendancePeriod { get; set; }
    public Employee Employee { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
