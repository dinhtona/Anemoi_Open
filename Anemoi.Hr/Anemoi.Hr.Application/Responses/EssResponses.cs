namespace Anemoi.Hr.Application.Responses;

public sealed class EssEmployeePortalAccessResponse
{
    public string Id { get; set; }
    public string EmployeeId { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? LastAccessAt { get; set; }
}

public sealed class EssEmployeeProfileResponse
{
    public string Id { get; set; }
    public string EmployeeCode { get; set; }
    public string FullName { get; set; }
    public string WorkEmail { get; set; }
    public string PhoneNumber { get; set; }
    public string DepartmentName { get; set; }
    public string PositionName { get; set; }
    public string GradeCode { get; set; }
    public string EmploymentStatusCode { get; set; }
    public DateOnly JoinDate { get; set; }
    public string ManagerName { get; set; }
}

public sealed class EssLeaveBalanceResponse
{
    public string LeavePolicyId { get; set; }
    public string LeavePolicyName { get; set; }
    public string LeaveTypeCode { get; set; }
    public int Year { get; set; }
    public decimal OpeningDays { get; set; }
    public decimal AccruedDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal PendingDays { get; set; }
    public decimal RemainingDays { get; set; }
}

public sealed class EssLeaveRequestResponse
{
    public string Id { get; set; }
    public string LeaveTypeCode { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal RequestedDays { get; set; }
    public string StatusCode { get; set; }
    public string Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class EssAttendanceRecordResponse
{
    public string Id { get; set; }
    public DateOnly WorkDate { get; set; }
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    public decimal WorkedHours { get; set; }
    public decimal WorkedDays { get; set; }
    public string Status { get; set; }
}

public sealed class EssAttendanceSummaryResponse
{
    public decimal WorkedDays { get; set; }
    public decimal LeaveDays { get; set; }
    public decimal AbsentDays { get; set; }
    public decimal HolidayDays { get; set; }
    public decimal WorkedHours { get; set; }
    public decimal PaidWorkingDays { get; set; }
    public decimal UnpaidLeaveDays { get; set; }
    public decimal PaidLeaveDays { get; set; }
}

public sealed class EssOvertimeRequestResponse
{
    public Guid Id { get; init; }
    public DateOnly OvertimeDate { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public decimal DurationHours { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public sealed class EssPayrollPeriodResponse
{
    public string Id { get; set; }
    public string PeriodCode { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string StatusCode { get; set; }
    public decimal StandardWorkingDays { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class EssPayslipResponse
{
    public string Id { get; set; }
    public string PayrollRunId { get; set; }
    public string PeriodCode { get; set; }
    public string EmployeeCode { get; set; }
    public string EmployeeName { get; set; }
    public decimal BaseSalarySnapshot { get; set; }
    public decimal DailyRateSnapshot { get; set; }
    public decimal PaidWorkingDays { get; set; }
    public decimal PaidLeaveDays { get; set; }
    public decimal UnpaidLeaveDays { get; set; }
    public decimal BasePayAmount { get; set; }
    public decimal AllowanceTotal { get; set; }
    public decimal DeductionTotal { get; set; }
    public decimal GrossPay { get; set; }
    public decimal NetPay { get; set; }
    public string Status { get; set; }
    public DateTime GeneratedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}
