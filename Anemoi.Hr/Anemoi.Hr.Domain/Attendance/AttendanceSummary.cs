using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Attendance;

public sealed class AttendanceSummary : ValueObject
{
    public AttendanceSummaryId Id { get; set; }
    public AttendancePeriodId AttendancePeriodId { get; set; }
    public EmployeeId EmployeeId { get; set; }
    public decimal WorkedDays { get; set; }
    public decimal WorkedHours { get; set; }
    public decimal LeaveDays { get; set; }
    public decimal AbsentDays { get; set; }
    public decimal HolidayDays { get; set; }
    public decimal PaidWorkingDays { get; set; }
    public decimal PaidLeaveDays { get; set; }
    public decimal UnpaidLeaveDays { get; set; }

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
