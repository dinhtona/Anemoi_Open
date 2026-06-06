namespace Anemoi.Hr.Application.Responses;

public sealed class AttendanceSummaryResponse
{
    public decimal WorkedDays { get; set; }
    public decimal LeaveDays { get; set; }
    public decimal AbsentDays { get; set; }
    public decimal HolidayDays { get; set; }
    public decimal WorkedHours { get; set; }

    // Temporary payroll-readiness fields until paid/unpaid leave classification is implemented.
    public decimal PaidWorkingDays { get; set; }
    public decimal UnpaidLeaveDays { get; set; }
    public decimal PaidLeaveDays { get; set; }
}
