namespace Anemoi.Hr.Application.Configurations;

public static class HrPermissions
{
    public const string LeavePolicyView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrLeavePolicyView;
    public const string LeavePolicyCreate = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrLeavePolicyCreate;
    public const string LeavePolicyUpdate = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrLeavePolicyUpdate;
    public const string LeaveBalanceView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrLeaveBalanceView;
    public const string LeaveBalanceAdjust = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrLeaveBalanceAdjust;
    public const string LeaveRequestView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrLeaveRequestView;
    public const string LeaveRequestCreate = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrLeaveRequestCreate;
    public const string LeaveRequestApprove =
        Anemoi.BuildingBlock.Application.Authorization.Permissions.HrLeaveRequestApprove;
    public const string LeaveRequestCancel =
        Anemoi.BuildingBlock.Application.Authorization.Permissions.HrLeaveRequestCancel;
    public const string LeaveRequestForceApprove =
        Anemoi.BuildingBlock.Application.Authorization.Permissions.HrLeaveRequestForceApprove;
    public const string LeaveRequestForceCancel =
        Anemoi.BuildingBlock.Application.Authorization.Permissions.HrLeaveRequestForceCancel;
    public const string LeaveTransactionView =
        Anemoi.BuildingBlock.Application.Authorization.Permissions.HrLeaveTransactionView;
    public const string EmployeeView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrEmployeeView;
    public const string EmployeeIdentityLink =
        Anemoi.BuildingBlock.Application.Authorization.Permissions.HrEmployeeIdentityLink;
    public const string DepartmentView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrDepartmentView;
    public const string DepartmentManage = "hr.department.manage";
    public const string PositionView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrPositionView;
    public const string PositionManage = "hr.position.manage";
    public const string DashboardView = "hr.dashboard.view";
    public const string AnalyticsView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrAnalyticsView;
    public const string EmployeeTransferView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrEmployeeTransferView;
    public const string EmployeeTransferCreate = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrEmployeeTransferCreate;
    public const string PromotionView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrPromotionView;
    public const string PromotionCreate = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrPromotionCreate;
    public const string PositionChangeView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrPositionChangeView;
    public const string PositionChange = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrPositionChange;
    public const string GradeChangeView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrGradeChangeView;
    public const string GradeChange = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrGradeChange;
    public const string ContractView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrContractView;
    public const string ContractCreate = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrContractCreate;
    public const string ContractUpdate = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrContractUpdate;
    public const string ContractTerminate = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrContractTerminate;

    public const string SalaryView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrSalaryView;
    public const string SalaryChange = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrSalaryChange;
    public const string SalaryGradeView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrSalaryGradeView;
    public const string SalaryGradeManage = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrSalaryGradeManage;
    public const string AllowanceTypeView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrAllowanceTypeView;
    public const string AllowanceTypeManage = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrAllowanceTypeManage;
    public const string EmployeeAllowanceView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrEmployeeAllowanceView;
    public const string EmployeeAllowanceChange = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrEmployeeAllowanceChange;
    public const string CompensationDashboardView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrCompensationDashboardView;
    public const string PayrollView = "hr.payroll.view";
    public const string PayrollCalculate = "hr.payroll.calculate";
    public const string PayrollApprove = "hr.payroll.approve";
    public const string PayrollLock = "hr.payroll.lock";

    public const string PayrollReportingView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrPayrollReportingView;
    public const string PayrollReportingExport = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrPayrollReportingExport;

    public const string PayslipDocumentView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrPayslipDocumentView;
    public const string PayslipDocumentGenerate = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrPayslipDocumentGenerate;
    public const string PayslipEmailSend = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrPayslipEmailSend;

    public const string AttendanceView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrAttendanceView;
    public const string AttendanceCreate = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrAttendanceCreate;
    public const string AttendanceUpdate = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrAttendanceUpdate;
    public const string AttendanceLock = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrAttendanceLock;
    public const string PayrollExport = "hr.payroll.export";
    public const string ShiftView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrShiftView;
    public const string ShiftManage = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrShiftManage;
    public const string ShiftAssign = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrShiftAssign;
    public const string ShiftCancel = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrShiftCancel;
    public const string OvertimeView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrOvertimeView;
    public const string OvertimeCreate = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrOvertimeRequest;
    public const string OvertimeApprove = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrOvertimeApprove;
    public const string OvertimeManage = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrOvertimeManage;
    public const string CalendarView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrCalendarView;
    public const string CalendarManage = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrCalendarManage;
    public const string TaxView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrTaxView;
    public const string TaxManage = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrTaxManage;
    public const string TaxCalculate = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrTaxCalculate;

    public const string InsuranceView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrInsuranceView;
    public const string InsuranceManage = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrInsuranceManage;
    public const string InsuranceCalculate = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrInsuranceCalculate;
    public const string InsuranceReport = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrInsuranceReport;

    public const string RecruitmentView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrRecruitmentView;
    public const string RecruitmentManage = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrRecruitmentManage;
    public const string RecruitmentInterview = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrRecruitmentInterview;
    public const string RecruitmentHire = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrRecruitmentHire;
    public const string RecruitmentAnalytics = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrRecruitmentAnalytics;
    public const string RecruitmentRequestView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrRecruitmentRequestView;
    public const string RecruitmentRequestCreate = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrRecruitmentRequestCreate;
    public const string RecruitmentRequestSubmit = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrRecruitmentRequestSubmit;
    public const string RecruitmentRequestApprove = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrRecruitmentRequestApprove;
    public const string RecruitmentRequestManage = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrRecruitmentRequestManage;

    public const string OnboardingView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrOnboardingView;
    public const string OnboardingManage = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrOnboardingManage;
    public const string OnboardingTaskComplete = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrOnboardingTaskComplete;
    public const string OnboardingTaskManage = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrOnboardingTaskManage;

    public const string WorkflowView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrWorkflowView;
    public const string WorkflowManage = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrWorkflowManage;
    public const string WorkflowExecute = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrWorkflowExecute;
    public const string WorkflowApprove = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrWorkflowApprove;

    public const string OrganizationView = "hr.organization.view";
    public const string OrganizationManage = "hr.organization.manage";
    public const string WorkflowOverride = "hr.workflow.override";

    public const string LeaveTypeView = "hr.leave.type.view";
    public const string LeaveTypeManage = "hr.leave.type.manage";
    public const string LeavePolicySettingsView = "hr.leave.policy.settings.view";
    public const string LeavePolicySettingsManage = "hr.leave.policy.settings.manage";
    public const string OvertimeRuleView = "hr.overtime.rule.view";
    public const string OvertimeRuleManage = "hr.overtime.rule.manage";

    public const string ProbationView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrProbationView;
    public const string ProbationManage = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrProbationManage;
    public const string SeparationView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrSeparationView;
    public const string SeparationCreate = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrSeparationCreate;
    public const string EmployeeTimelineView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrEmployeeTimelineView;

    public const string EssProfileView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrEssProfileView;
    public const string EssLeaveView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrEssLeaveView;
    public const string EssLeaveRequest = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrEssLeaveRequest;
    public const string EssAttendanceView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrEssAttendanceView;
    public const string EssOvertimeView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrEssOvertimeView;
    public const string EssOvertimeCreate = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrEssOvertimeCreate;
    public const string EssPayrollView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrEssPayrollView;
    public const string EssPayslipView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrEssPayslipView;

    public static readonly IReadOnlyList<string> All =
    [
        LeavePolicyView,
        LeavePolicyCreate,
        LeavePolicyUpdate,
        LeaveBalanceView,
        LeaveBalanceAdjust,
        LeaveRequestView,
        LeaveRequestCreate,
        LeaveRequestApprove,
        LeaveRequestCancel,
        LeaveRequestForceApprove,
        LeaveRequestForceCancel,
        LeaveTransactionView,
        EmployeeView,
        EmployeeIdentityLink,
        DepartmentView,
        DepartmentManage,
        PositionView,
        PositionManage,
        DashboardView,
        AnalyticsView,
        EmployeeTransferView,
        EmployeeTransferCreate,
        PromotionView,
        PromotionCreate,
        PositionChangeView,
        PositionChange,
        GradeChangeView,
        GradeChange,
        ContractView,
        ContractCreate,
        ContractUpdate,
        ContractTerminate,
        SalaryView,
        SalaryChange,
        SalaryGradeView,
        SalaryGradeManage,
        AllowanceTypeView,
        AllowanceTypeManage,
        EmployeeAllowanceView,
        EmployeeAllowanceChange,
        CompensationDashboardView,
        PayrollView,
        PayrollCalculate,
        PayrollApprove,
        PayrollLock,
        PayrollReportingView,
        PayrollReportingExport,
        PayslipDocumentView,
        PayslipDocumentGenerate,
        PayslipEmailSend,
        PayrollExport,
        AttendanceView,
        AttendanceCreate,
        AttendanceUpdate,
        AttendanceLock,
        ShiftView,
        ShiftManage,
        ShiftAssign,
        ShiftCancel,
        OvertimeView,
        OvertimeCreate,
        OvertimeApprove,
        OvertimeManage,
        CalendarView,
        CalendarManage,
        TaxView,
        TaxManage,
        TaxCalculate,
        InsuranceView,
        InsuranceManage,
        InsuranceCalculate,
        InsuranceReport,
        RecruitmentView,
        RecruitmentManage,
        RecruitmentInterview,
        RecruitmentHire,
        RecruitmentAnalytics,
        RecruitmentRequestView,
        RecruitmentRequestCreate,
        RecruitmentRequestSubmit,
        RecruitmentRequestApprove,
        RecruitmentRequestManage,
        OnboardingView,
        OnboardingManage,
        OnboardingTaskComplete,
        OnboardingTaskManage,
        WorkflowView,
        WorkflowManage,
        WorkflowExecute,
        WorkflowApprove,
        EssProfileView,
        EssLeaveView,
        EssLeaveRequest,
        EssAttendanceView,
        EssOvertimeView,
        EssOvertimeCreate,
        EssPayrollView,
        EssPayslipView,
        OrganizationView,
        OrganizationManage,
        LeaveTypeView,
        LeaveTypeManage,
        LeavePolicySettingsView,
        LeavePolicySettingsManage,
        OvertimeRuleView,
        OvertimeRuleManage,
        WorkflowOverride,
        ProbationView,
        ProbationManage,
        SeparationCreate,
        SeparationView,
        EmployeeTimelineView
    ];

    public static readonly IReadOnlyDictionary<string, SensitivePermissionDefinition> SensitivePermissions =
        new Dictionary<string, SensitivePermissionDefinition>
        {
            [LeaveBalanceAdjust] = new(LeaveBalanceAdjust, "High"),
            [LeaveRequestForceApprove] = new(LeaveRequestForceApprove, "High"),
            [LeaveRequestForceCancel] = new(LeaveRequestForceCancel, "High"),
            [EmployeeIdentityLink] = new(EmployeeIdentityLink, "High"),
            [EmployeeTransferCreate] = new(EmployeeTransferCreate, "High"),
            [PromotionCreate] = new(PromotionCreate, "High"),
            [PositionChange] = new(PositionChange, "High"),
            [GradeChange] = new(GradeChange, "High"),
            [ContractView] = new(ContractView, "High"),
            [ContractCreate] = new(ContractCreate, "High"),
            [ContractUpdate] = new(ContractUpdate, "High"),
            [ContractTerminate] = new(ContractTerminate, "High"),
            [SalaryView] = new(SalaryView, "High"),
            [SalaryChange] = new(SalaryChange, "High"),
            [SalaryGradeManage] = new(SalaryGradeManage, "High"),
            [AllowanceTypeManage] = new(AllowanceTypeManage, "High"),
            [EmployeeAllowanceChange] = new(EmployeeAllowanceChange, "High"),
            [PayrollCalculate] = new(PayrollCalculate, "High"),
            [PayrollApprove] = new(PayrollApprove, "High"),
            [PayrollLock] = new(PayrollLock, "Critical"),
            [PayrollExport] = new(PayrollExport, "High"),
            [AttendanceLock] = new(AttendanceLock, "High"),
            [TaxManage] = new(TaxManage, "High"),
            [InsuranceManage] = new(InsuranceManage, "High"),
            [PayslipDocumentGenerate] = new(PayslipDocumentGenerate, "High"),
            [PayslipEmailSend] = new(PayslipEmailSend, "High"),
            [PayrollReportingExport] = new(PayrollReportingExport, "High"),
            [RecruitmentManage] = new(RecruitmentManage, "High"),
            [RecruitmentHire] = new(RecruitmentHire, "High"),
            [RecruitmentRequestSubmit] = new(RecruitmentRequestSubmit, "High"),
            [RecruitmentRequestApprove] = new(RecruitmentRequestApprove, "High"),
            [RecruitmentRequestManage] = new(RecruitmentRequestManage, "High"),
            [OnboardingManage] = new(OnboardingManage, "High"),
            [OnboardingTaskManage] = new(OnboardingTaskManage, "High"),
            [WorkflowApprove] = new(WorkflowApprove, "High"),
            [OrganizationManage] = new(OrganizationManage, "High")
        };
}

public sealed record SensitivePermissionDefinition(string Code, string RiskLevel);
