namespace Anemoi.Notification.Application.Constants;

/// <summary>
/// Centralized DataChange resource keys emitted by notification consumers.
/// Replaces scattered string literals (CODE-010 / MED-16).
/// </summary>
public static class NotificationResourceKeys
{
    public const string HrEmployee = "hr.employee";
    public const string HrEmployeeTransfer = "hr.employee.transfer";
    public const string HrEmployeeSeparation = "hr.employee.separation";
    public const string HrLeaveRequest = "hr.leave.request";
    public const string HrLeaveBalance = "hr.leave.balance";
    public const string HrOvertimeRequest = "hr.overtime.request";
    public const string HrPayroll = "hr.payroll";
    public const string HrPayslip = "hr.payslip";
    public const string HrRecruitmentRequest = "hr.recruitment.request";
    public const string HrRecruitmentOpening = "hr.recruitment.opening";
}
