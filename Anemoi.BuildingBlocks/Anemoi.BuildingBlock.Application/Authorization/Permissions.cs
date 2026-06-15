using System.Collections.Generic;
using System.Linq;

namespace Anemoi.BuildingBlock.Application.Authorization;

public static class Permissions
{
    public sealed record Definition(string Key, string GroupKey, string DescriptionKey, bool IsSensitive = false,
        string RiskLevel = null);

    public const string UserRead = "UserQuery";
    public const string UserManage = "UserCommand";
    public const string RoleRead = "RoleQuery";
    public const string RoleManage = "RoleCommand";

    public const string SeedGeneratorRead = "SeedGeneratorRead";
    public const string SeedGeneratorManage = "SeedGeneratorManage";
    public const string SeedExecutionRun = "SeedExecutionRun";
    public const string SeedExecutionManualWrite = "SeedExecutionManualWrite";
    public const string SeedExecutionDeleteRow = "SeedExecutionDeleteRow";
    public const string SeedExecutionLogClear = "SeedExecutionLogClear";

    public const string EnvironmentRead = "EnvironmentRead";
    public const string EnvironmentStartStop = "EnvironmentStartStop";
    public const string EnvironmentSftpRead = "EnvironmentSftpRead";
    public const string EnvironmentSftpManage = "EnvironmentSftpManage";
    public const string EnvironmentMockRouteRead = "EnvironmentMockRouteRead";
    public const string EnvironmentMockRouteManage = "EnvironmentMockRouteManage";

    public const string HrLeavePolicyView = "hr.leave.policy.view";
    public const string HrLeavePolicyCreate = "hr.leave.policy.create";
    public const string HrLeavePolicyUpdate = "hr.leave.policy.update";
    public const string HrLeaveBalanceView = "hr.leave.balance.view";
    public const string HrLeaveBalanceAdjust = "hr.leave.balance.adjust";
    public const string HrLeaveRequestView = "hr.leave.request.view";
    public const string HrLeaveRequestCreate = "hr.leave.request.create";
    public const string HrLeaveRequestApprove = "hr.leave.request.approve";
    public const string HrLeaveRequestCancel = "hr.leave.request.cancel";
    public const string HrLeaveRequestForceApprove = "hr.leave.request.force_approve";
    public const string HrLeaveRequestForceCancel = "hr.leave.request.force_cancel";
    public const string HrLeaveTransactionView = "hr.leave.transaction.view";
    public const string HrEmployeeView = "hr.employee.view";
    public const string HrEmployeeIdentityLink = "hr.employee.identity_link";
    public const string HrDepartmentView = "hr.department.view";
    public const string HrPositionView = "hr.position.view";
    public const string HrDashboardView = "hr.dashboard.view";
    public const string HrAnalyticsView = "hr.analytics.view";
    public const string HrEmployeeTransferView = "hr.employee.transfer.view";
    public const string HrEmployeeTransferCreate = "hr.employee.transfer.create";
    public const string HrPromotionView = "hr.promotion.view";
    public const string HrPromotionCreate = "hr.promotion.create";
    public const string HrPositionChangeView = "hr.position.change.view";
    public const string HrPositionChange = "hr.position.change";
    public const string HrGradeChangeView = "hr.grade.change.view";
    public const string HrGradeChange = "hr.grade.change";
    public const string HrContractView = "hr.contract.view";
    public const string HrContractCreate = "hr.contract.create";
    public const string HrContractUpdate = "hr.contract.update";
    public const string HrContractTerminate = "hr.contract.terminate";
    
    public const string HrPayslipDocumentView = "hr.payslip.document.view";
    public const string HrPayslipDocumentGenerate = "hr.payslip.document.generate";
    public const string HrPayslipEmailSend = "hr.payslip.email.send";

    public const string HrSalaryView = "hr.salary.view";
    public const string HrSalaryChange = "hr.salary.change";
    public const string HrSalaryGradeView = "hr.salary.grade.view";
    public const string HrSalaryGradeManage = "hr.salary.grade.manage";
    public const string HrAllowanceTypeView = "hr.allowance.type.view";
    public const string HrAllowanceTypeManage = "hr.allowance.type.manage";
    public const string HrEmployeeAllowanceView = "hr.employee.allowance.view";
    public const string HrEmployeeAllowanceChange = "hr.employee.allowance.change";
    public const string HrCompensationDashboardView = "hr.compensation.dashboard.view";

    public const string HrAttendanceView = "hr.attendance.view";
    public const string HrOvertimeView = "hr.overtime.view";
    public const string HrOvertimeRequest = "hr.overtime.request";
    public const string HrOvertimeApprove = "hr.overtime.approve";
    public const string HrOvertimeManage = "hr.overtime.manage";
    public const string HrAttendanceCreate = "hr.attendance.create";
    public const string HrAttendanceUpdate = "hr.attendance.update";
    public const string HrAttendanceLock = "hr.attendance.lock";
    public const string HrShiftView = "hr.shift.view";
    public const string HrShiftManage = "hr.shift.manage";
    public const string HrShiftAssign = "hr.shift.assign";
    public const string HrShiftCancel = "hr.shift.cancel";
    public const string HrCalendarView = "hr.calendar.view";
    public const string HrCalendarManage = "hr.calendar.manage";

    public const string HrTaxView = "hr.tax.view";
    public const string HrTaxManage = "hr.tax.manage";
    public const string HrTaxCalculate = "hr.tax.calculate";

    public const string HrInsuranceView = "hr.insurance.view";
    public const string HrInsuranceManage = "hr.insurance.manage";
    public const string HrInsuranceCalculate = "hr.insurance.calculate";
    public const string HrInsuranceReport = "hr.insurance.report";

    public const string HrPayrollView = "hr.payroll.view";
    public const string HrPayrollCalculate = "hr.payroll.calculate";
    public const string HrPayrollApprove = "hr.payroll.approve";
    public const string HrPayrollLock = "hr.payroll.lock";
    public const string HrPayrollExport = "hr.payroll.export";
    public const string HrPayrollReportingView = "hr.payroll.reporting.view";
    public const string HrPayrollReportingExport = "hr.payroll.reporting.export";

    public const string HrEssProfileView = "hr.ess.profile.view";
    public const string HrEssLeaveView = "hr.ess.leave.view";
    public const string HrEssLeaveRequest = "hr.ess.leave.request";
    public const string HrEssAttendanceView = "hr.ess.attendance.view";
    public const string HrEssOvertimeView = "hr.ess.overtime.view";
    public const string HrEssOvertimeCreate = "hr.ess.overtime.create";
    public const string HrEssPayrollView = "hr.ess.payroll.view";
    public const string HrEssPayslipView = "hr.ess.payslip.view";

    public static readonly IReadOnlyList<Definition> Definitions =
    [
        new(UserRead, "PermissionGroupUsers", "PermissionDescriptionUserRead"),
        new(UserManage, "PermissionGroupUsers", "PermissionDescriptionUserManage"),
        new(RoleRead, "PermissionGroupRoleGroups", "PermissionDescriptionRoleRead"),
        new(RoleManage, "PermissionGroupRoleGroups", "PermissionDescriptionRoleManage"),
        new(SeedGeneratorRead, "PermissionGroupSeedGenerator", "PermissionDescriptionSeedGeneratorRead"),
        new(SeedGeneratorManage, "PermissionGroupSeedGenerator", "PermissionDescriptionSeedGeneratorManage"),
        new(SeedExecutionRun, "PermissionGroupSeedGenerator", "PermissionDescriptionSeedExecutionRun"),
        new(SeedExecutionManualWrite, "PermissionGroupSeedGenerator", "PermissionDescriptionSeedExecutionManualWrite"),
        new(SeedExecutionDeleteRow, "PermissionGroupSeedGenerator", "PermissionDescriptionSeedExecutionDeleteRow"),
        new(SeedExecutionLogClear, "PermissionGroupSeedGenerator", "PermissionDescriptionSeedExecutionLogClear"),
        new(EnvironmentRead, "PermissionGroupEnvironments", "PermissionDescriptionEnvironmentRead"),
        new(EnvironmentStartStop, "PermissionGroupEnvironments", "PermissionDescriptionEnvironmentStartStop"),
        new(EnvironmentSftpRead, "PermissionGroupEnvironments", "PermissionDescriptionEnvironmentSftpRead"),
        new(EnvironmentSftpManage, "PermissionGroupEnvironments", "PermissionDescriptionEnvironmentSftpManage"),
        new(EnvironmentMockRouteRead, "PermissionGroupEnvironments", "PermissionDescriptionEnvironmentMockRouteRead"),
        new(EnvironmentMockRouteManage, "PermissionGroupEnvironments", "PermissionDescriptionEnvironmentMockRouteManage"),
        new(HrLeavePolicyView, "PermissionGroupHrLeave", "PermissionDescriptionHrLeavePolicyView"),
        new(HrLeavePolicyCreate, "PermissionGroupHrLeave", "PermissionDescriptionHrLeavePolicyCreate"),
        new(HrLeavePolicyUpdate, "PermissionGroupHrLeave", "PermissionDescriptionHrLeavePolicyUpdate"),
        new(HrLeaveBalanceView, "PermissionGroupHrLeave", "PermissionDescriptionHrLeaveBalanceView"),
        new(HrLeaveBalanceAdjust, "PermissionGroupHrLeave", "PermissionDescriptionHrLeaveBalanceAdjust", true, "High"),
        new(HrLeaveRequestView, "PermissionGroupHrLeave", "PermissionDescriptionHrLeaveRequestView"),
        new(HrLeaveRequestCreate, "PermissionGroupHrLeave", "PermissionDescriptionHrLeaveRequestCreate"),
        new(HrLeaveRequestApprove, "PermissionGroupHrLeave", "PermissionDescriptionHrLeaveRequestApprove"),
        new(HrLeaveRequestCancel, "PermissionGroupHrLeave", "PermissionDescriptionHrLeaveRequestCancel"),
        new(HrLeaveRequestForceApprove, "PermissionGroupHrLeave",
            "PermissionDescriptionHrLeaveRequestForceApprove", true, "High"),
        new(HrLeaveRequestForceCancel, "PermissionGroupHrLeave",
            "PermissionDescriptionHrLeaveRequestForceCancel", true, "High"),
        new(HrLeaveTransactionView, "PermissionGroupHrLeave", "PermissionDescriptionHrLeaveTransactionView"),
        new(HrEmployeeView, "PermissionGroupHrEmployee", "PermissionDescriptionHrEmployeeView"),
        new(HrEmployeeIdentityLink, "PermissionGroupHrEmployee", "PermissionDescriptionHrEmployeeIdentityLink",
            true, "High"),
        new(HrDepartmentView, "PermissionGroupHrDepartment", "PermissionDescriptionHrDepartmentView"),
        new(HrPositionView, "PermissionGroupHrPosition", "PermissionDescriptionHrPositionView"),
        new(HrDashboardView, "PermissionGroupHrEmployee", "PermissionDescriptionHrDashboardView"),
        new(HrAnalyticsView, "PermissionGroupHrEmployee", "PermissionDescriptionHrAnalyticsView"),
        new(HrEmployeeTransferView, "PermissionGroupHrEmployee", "PermissionDescriptionHrEmployeeTransferView"),
        new(HrEmployeeTransferCreate, "PermissionGroupHrEmployee", "PermissionDescriptionHrEmployeeTransferCreate", true, "High"),
        new(HrPromotionView, "PermissionGroupHrEmployee", "PermissionDescriptionHrPromotionView"),
        new(HrPromotionCreate, "PermissionGroupHrEmployee", "PermissionDescriptionHrPromotionCreate", true, "High"),
        new(HrPositionChangeView, "PermissionGroupHrPosition", "PermissionDescriptionHrPositionChangeView"),
        new(HrPositionChange, "PermissionGroupHrPosition", "PermissionDescriptionHrPositionChange", true, "High"),
        new(HrGradeChangeView, "PermissionGroupHrEmployee", "PermissionDescriptionHrGradeChangeView"),
        new(HrGradeChange, "PermissionGroupHrEmployee", "PermissionDescriptionHrGradeChange", true, "High"),
        new(HrContractView, "PermissionGroupHrEmployee", "PermissionDescriptionHrContractView", true, "High"),
        new(HrContractCreate, "PermissionGroupHrEmployee", "PermissionDescriptionHrContractCreate", true, "High"),
        new(HrContractUpdate, "PermissionGroupHrEmployee", "PermissionDescriptionHrContractUpdate", true, "High"),
        new(HrContractTerminate, "PermissionGroupHrEmployee", "PermissionDescriptionHrContractTerminate", true, "High"),
        new(HrSalaryView, "PermissionGroupHrEmployee", "PermissionDescriptionHrSalaryView", true, "High"),
        new(HrSalaryChange, "PermissionGroupHrEmployee", "PermissionDescriptionHrSalaryChange", true, "High"),
        new(HrSalaryGradeView, "PermissionGroupHrEmployee", "PermissionDescriptionHrSalaryGradeView"),
        new(HrSalaryGradeManage, "PermissionGroupHrEmployee", "PermissionDescriptionHrSalaryGradeManage", true, "High"),
        new(HrAllowanceTypeView, "PermissionGroupHrEmployee", "PermissionDescriptionHrAllowanceTypeView"),
        new(HrAllowanceTypeManage, "PermissionGroupHrEmployee", "PermissionDescriptionHrAllowanceTypeManage", true, "High"),
        new(HrEmployeeAllowanceView, "PermissionGroupHrEmployee", "PermissionDescriptionHrEmployeeAllowanceView"),
        new(HrEmployeeAllowanceChange, "PermissionGroupHrEmployee", "PermissionDescriptionHrEmployeeAllowanceChange", true, "High"),
        new(HrCompensationDashboardView, "PermissionGroupHrEmployee", "PermissionDescriptionHrCompensationDashboardView"),
        new(HrAttendanceView, "PermissionGroupHrAttendance", "PermissionDescriptionHrAttendanceView"),
        new(HrAttendanceCreate, "PermissionGroupHrAttendance", "PermissionDescriptionHrAttendanceCreate"),
        new(HrAttendanceUpdate, "PermissionGroupHrAttendance", "PermissionDescriptionHrAttendanceUpdate"),
        new(HrAttendanceLock, "PermissionGroupHrAttendance", "PermissionDescriptionHrAttendanceLock", true, "High"),
        new(HrOvertimeView, "PermissionGroupHrOvertime", "PermissionDescriptionHrOvertimeView"),
        new(HrOvertimeRequest, "PermissionGroupHrOvertime", "PermissionDescriptionHrOvertimeRequest"),
        new(HrOvertimeApprove, "PermissionGroupHrOvertime", "PermissionDescriptionHrOvertimeApprove"),
        new(HrOvertimeManage, "PermissionGroupHrOvertime", "PermissionDescriptionHrOvertimeManage", true, "High"),
        new(HrShiftView, "PermissionGroupHrShift", "PermissionDescriptionHrShiftView"),
        new(HrShiftManage, "PermissionGroupHrShift", "PermissionDescriptionHrShiftManage"),
        new(HrShiftAssign, "PermissionGroupHrShift", "PermissionDescriptionHrShiftAssign"),
        new(HrShiftCancel, "PermissionGroupHrShift", "PermissionDescriptionHrShiftCancel"),
        new(HrCalendarView, "PermissionGroupHrCalendar", "PermissionDescriptionHrCalendarView"),
        new(HrCalendarManage, "PermissionGroupHrCalendar", "PermissionDescriptionHrCalendarManage"),
        new(HrTaxView, "PermissionGroupHrTax", "PermissionDescriptionHrTaxView"),
        new(HrTaxManage, "PermissionGroupHrTax", "PermissionDescriptionHrTaxManage", true, "High"),
        new(HrTaxCalculate, "PermissionGroupHrTax", "PermissionDescriptionHrTaxCalculate"),
        new(HrInsuranceView, "PermissionGroupHrInsurance", "PermissionDescriptionHrInsuranceView"),
        new(HrInsuranceManage, "PermissionGroupHrInsurance", "PermissionDescriptionHrInsuranceManage", true, "High"),
        new(HrInsuranceCalculate, "PermissionGroupHrInsurance", "PermissionDescriptionHrInsuranceCalculate"),
        new(HrInsuranceReport, "PermissionGroupHrInsurance", "PermissionDescriptionHrInsuranceReport"),
        new(HrPayrollView, "PermissionGroupHrPayroll", "PermissionDescriptionHrPayrollView"),
        new(HrPayrollCalculate, "PermissionGroupHrPayroll", "PermissionDescriptionHrPayrollCalculate", true, "High"),
        new(HrPayrollApprove, "PermissionGroupHrPayroll", "PermissionDescriptionHrPayrollApprove", true, "High"),
        new(HrPayrollLock, "PermissionGroupHrPayroll", "PermissionDescriptionHrPayrollLock", true, "Critical"),
        new(HrPayrollExport, "PermissionGroupHrPayroll", "PermissionDescriptionHrPayrollExport", true, "High"),
        new(HrPayslipDocumentView, "PermissionGroupHrPayroll", "PermissionDescriptionHrPayslipDocumentView"),
        new(HrPayslipDocumentGenerate, "PermissionGroupHrPayroll", "PermissionDescriptionHrPayslipDocumentGenerate"),
        new(HrPayslipEmailSend, "PermissionGroupHrPayroll", "PermissionDescriptionHrPayslipEmailSend"),
        new(HrPayrollReportingView, "PermissionGroupHrPayroll", "PermissionDescriptionHrPayrollReportingView"),
        new(HrPayrollReportingExport, "PermissionGroupHrPayroll", "PermissionDescriptionHrPayrollReportingExport", true, "High"),
        new(HrEssProfileView, "PermissionGroupHrEss", "PermissionDescriptionHrEssProfileView"),
        new(HrEssLeaveView, "PermissionGroupHrEss", "PermissionDescriptionHrEssLeaveView"),
        new(HrEssLeaveRequest, "PermissionGroupHrEss", "PermissionDescriptionHrEssLeaveRequest"),
        new(HrEssAttendanceView, "PermissionGroupHrEss", "PermissionDescriptionHrEssAttendanceView"),
        new(HrEssOvertimeView, "PermissionGroupHrEss", "PermissionDescriptionHrEssOvertimeView"),
        new(HrEssOvertimeCreate, "PermissionGroupHrEss", "PermissionDescriptionHrEssOvertimeCreate"),
        new(HrEssPayrollView, "PermissionGroupHrEss", "PermissionDescriptionHrEssPayrollView"),
        new(HrEssPayslipView, "PermissionGroupHrEss", "PermissionDescriptionHrEssPayslipView")
    ];

    public static readonly IReadOnlyList<string> All = Definitions.Select(x => x.Key).ToList();

    public static Definition Find(string key) => Definitions.FirstOrDefault(x => x.Key == key);
}
