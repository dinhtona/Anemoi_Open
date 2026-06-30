using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Attendance;
using Anemoi.Hr.Domain.Ess;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Leaves;
using Anemoi.Hr.Domain.Overtime;
using Anemoi.Hr.Domain.Payroll;
using Riok.Mapperly.Abstractions;

namespace Anemoi.Hr.Application.Mappings;

[Mapper]
public partial class EssMapper
{
    public EssEmployeePortalAccessResponse ToEmployeePortalAccessResponse(EmployeePortalAccess source)
    {
        return new EssEmployeePortalAccessResponse
        {
            Id = source.Id.Value.ToString(),
            EmployeeId = source.EmployeeId.Value.ToString(),
            LastLoginAt = source.LastLoginAt,
            LastAccessAt = source.LastAccessAt
        };
    }

    public EssEmployeeProfileResponse ToEssEmployeeProfileResponse(Employee employee)
    {
        return new EssEmployeeProfileResponse
        {
            Id = employee.Id.Value.ToString(),
            EmployeeCode = employee.EmployeeCode,
            FullName = employee.FullName,
            WorkEmail = employee.WorkEmail,
            PhoneNumber = employee.PhoneNumber,
            DepartmentName = employee.PrimaryDepartment?.Name,
            PositionName = employee.PrimaryPosition?.Name,
            GradeCode = employee.GradeCode,
            EmploymentStatusCode = employee.EmploymentStatusCode,
            JoinDate = employee.JoinDate,
            ManagerName = employee.DirectManager?.FullName
        };
    }

    public EssLeaveBalanceResponse ToEssLeaveBalanceResponse(LeaveBalance balance)
    {
        return new EssLeaveBalanceResponse
        {
            LeavePolicyId = balance.LeavePolicyId.Value.ToString(),
            LeavePolicyName = balance.LeavePolicy?.Name,
            LeaveTypeCode = balance.LeavePolicy?.LeaveType?.Code,
            Year = balance.Year,
            OpeningDays = balance.OpeningDays,
            AccruedDays = balance.AccruedDays,
            UsedDays = balance.UsedDays,
            PendingDays = balance.PendingDays,
            RemainingDays = balance.RemainingDays
        };
    }

    public EssLeaveRequestResponse ToEssLeaveRequestResponse(LeaveRequest request)
    {
        return new EssLeaveRequestResponse
        {
            Id = request.Id.Value.ToString(),
            LeaveTypeCode = request.LeaveTypeCode,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            RequestedDays = request.RequestedDays,
            StatusCode = request.StatusCode,
            Reason = request.Reason,
            CreatedAt = request.CreatedAt,
            UpdatedAt = request.UpdatedAt
        };
    }

    public EssOvertimeRequestResponse ToEssOvertimeRequestResponse(OvertimeRequest request)
    {
        return new EssOvertimeRequestResponse
        {
            Id = request.Id.Value,
            OvertimeDate = request.OvertimeDate,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            DurationHours = request.CalculateDurationHours(),
            Reason = request.Reason,
            Status = request.Status,
            CreatedAt = request.CreatedAt
        };
    }

    public EssPayrollPeriodResponse ToEssPayrollPeriodResponse(PayrollPeriod period)
    {
        return new EssPayrollPeriodResponse
        {
            Id = period.Id.Value.ToString(),
            PeriodCode = period.PeriodCode,
            StartDate = period.StartDate,
            EndDate = period.EndDate,
            StatusCode = period.StatusCode,
            StandardWorkingDays = period.StandardWorkingDays,
            CreatedAt = period.CreatedAt
        };
    }

    public EssPayslipResponse ToEssPayslipResponse(Payslip payslip)
    {
        return new EssPayslipResponse
        {
            Id = payslip.Id.Value.ToString(),
            PayrollRunId = payslip.PayrollRunId.Value.ToString(),
            PeriodCode = payslip.PeriodCode,
            EmployeeCode = payslip.EmployeeCode,
            EmployeeName = payslip.EmployeeName,
            BaseSalarySnapshot = payslip.BaseSalarySnapshot,
            DailyRateSnapshot = payslip.DailyRateSnapshot,
            PaidWorkingDays = payslip.PaidWorkingDays,
            PaidLeaveDays = payslip.PaidLeaveDays,
            UnpaidLeaveDays = payslip.UnpaidLeaveDays,
            BasePayAmount = payslip.BasePayAmount,
            AllowanceTotal = payslip.AllowanceTotal,
            DeductionTotal = payslip.DeductionTotal,
            GrossPay = payslip.GrossPay,
            NetPay = payslip.NetPay,
            Status = payslip.Status.ToString(),
            GeneratedAt = payslip.GeneratedAt,
            PublishedAt = payslip.PublishedAt
        };
    }

    public EssAttendanceRecordResponse ToEssAttendanceRecordResponse(AttendanceRecord record)
    {
        return new EssAttendanceRecordResponse
        {
            Id = record.Id.Value.ToString(),
            WorkDate = record.WorkDate,
            CheckInTime = record.CheckInTime,
            CheckOutTime = record.CheckOutTime,
            WorkedHours = record.WorkedHours,
            WorkedDays = record.WorkedDays,
            Status = record.Status
        };
    }
}
