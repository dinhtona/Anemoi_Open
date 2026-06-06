using System;

namespace Anemoi.Hr.Application.Responses;

public class AttendanceRecordResponse
{
    public string Id { get; set; }
    public string AttendancePeriodId { get; set; }
    public string EmployeeId { get; set; }
    public string EmployeeCode { get; set; }
    public string EmployeeName { get; set; }
    public DateOnly WorkDate { get; set; }
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    public decimal WorkedHours { get; set; }
    public decimal WorkedDays { get; set; }
    public string Status { get; set; }
    public string LeaveRequestId { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
}
