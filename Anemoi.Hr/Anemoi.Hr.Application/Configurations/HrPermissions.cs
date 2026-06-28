namespace Anemoi.Hr.Application.Configurations;

using BB = Anemoi.BuildingBlock.Application.Authorization.Permissions;

public static class HrPermissions
{
    public const string LeavePolicyView = BB.HrLeavePolicyView;
    public const string LeavePolicyCreate = BB.HrLeavePolicyCreate;
    public const string LeavePolicyUpdate = BB.HrLeavePolicyUpdate;
    public const string LeaveBalanceView = BB.HrLeaveBalanceView;
    public const string LeaveBalanceAdjust = BB.HrLeaveBalanceAdjust;
    public const string LeaveRequestView = BB.HrLeaveRequestView;
    public const string LeaveRequestCreate = BB.HrLeaveRequestCreate;
    public const string LeaveRequestApprove =
        BB.HrLeaveRequestApprove;
    public const string LeaveRequestCancel =
        BB.HrLeaveRequestCancel;
    public const string LeaveRequestForceApprove =
        BB.HrLeaveRequestForceApprove;
    public const string LeaveRequestForceCancel =
        BB.HrLeaveRequestForceCancel;
    public const string LeaveTransactionView =
        BB.HrLeaveTransactionView;
    public const string EmployeeView = BB.HrEmployeeView;
    public const string EmployeeIdentityLink =
        BB.HrEmployeeIdentityLink;
    public const string DepartmentView = BB.HrDepartmentView;
    public const string DepartmentManage = BB.HrDepartmentManage;
    public const string PositionView = BB.HrPositionView;
    public const string PositionManage = BB.HrPositionManage;
    public const string DashboardView = BB.HrDashboardView;
    public const string AnalyticsView = BB.HrAnalyticsView;
    public const string EmployeeTransferView = BB.HrEmployeeTransferView;
    public const string EmployeeTransferCreate = BB.HrEmployeeTransferCreate;
    public const string PromotionView = BB.HrPromotionView;
    public const string PromotionCreate = BB.HrPromotionCreate;
    public const string PositionChangeView = BB.HrPositionChangeView;
    public const string PositionChange = BB.HrPositionChange;
    public const string GradeChangeView = BB.HrGradeChangeView;
    public const string GradeChange = BB.HrGradeChange;
    public const string ContractView = BB.HrContractView;
    public const string ContractCreate = BB.HrContractCreate;
    public const string ContractUpdate = BB.HrContractUpdate;
    public const string ContractTerminate = BB.HrContractTerminate;

    public const string SalaryView = BB.HrSalaryView;
    public const string SalaryChange = BB.HrSalaryChange;
    public const string SalaryGradeView = BB.HrSalaryGradeView;
    public const string SalaryGradeManage = BB.HrSalaryGradeManage;
    public const string AllowanceTypeView = BB.HrAllowanceTypeView;
    public const string AllowanceTypeManage = BB.HrAllowanceTypeManage;
    public const string EmployeeAllowanceView = BB.HrEmployeeAllowanceView;
    public const string EmployeeAllowanceChange = BB.HrEmployeeAllowanceChange;
    public const string CompensationDashboardView = BB.HrCompensationDashboardView;
    public const string PayrollView = BB.HrPayrollView;
    public const string PayrollCalculate = BB.HrPayrollCalculate;
    public const string PayrollApprove = BB.HrPayrollApprove;
    public const string PayrollLock = BB.HrPayrollLock;

    public const string PayrollReportingView = BB.HrPayrollReportingView;
    public const string PayrollReportingExport = BB.HrPayrollReportingExport;

    public const string PayslipDocumentView = BB.HrPayslipDocumentView;
    public const string PayslipDocumentGenerate = BB.HrPayslipDocumentGenerate;
    public const string PayslipEmailSend = BB.HrPayslipEmailSend;

    public const string AttendanceView = BB.HrAttendanceView;
    public const string AttendanceCreate = BB.HrAttendanceCreate;
    public const string AttendanceUpdate = BB.HrAttendanceUpdate;
    public const string AttendanceLock = BB.HrAttendanceLock;
    public const string PayrollExport = BB.HrPayrollExport;
    public const string ShiftView = BB.HrShiftView;
    public const string ShiftManage = BB.HrShiftManage;
    public const string ShiftAssign = BB.HrShiftAssign;
    public const string ShiftCancel = BB.HrShiftCancel;
    public const string OvertimeView = BB.HrOvertimeView;
    public const string OvertimeCreate = BB.HrOvertimeRequest;
    public const string OvertimeApprove = BB.HrOvertimeApprove;
    public const string OvertimeManage = BB.HrOvertimeManage;
    public const string CalendarView = BB.HrCalendarView;
    public const string CalendarManage = BB.HrCalendarManage;
    public const string TaxView = BB.HrTaxView;
    public const string TaxManage = BB.HrTaxManage;
    public const string TaxCalculate = BB.HrTaxCalculate;

    public const string InsuranceView = BB.HrInsuranceView;
    public const string InsuranceManage = BB.HrInsuranceManage;
    public const string InsuranceCalculate = BB.HrInsuranceCalculate;
    public const string InsuranceReport = BB.HrInsuranceReport;

    public const string RecruitmentView = BB.HrRecruitmentView;
    public const string RecruitmentManage = BB.HrRecruitmentManage;
    public const string RecruitmentInterview = BB.HrRecruitmentInterview;
    public const string RecruitmentHire = BB.HrRecruitmentHire;
    public const string RecruitmentAnalytics = BB.HrRecruitmentAnalytics;
    public const string RecruitmentRequestView = BB.HrRecruitmentRequestView;
    public const string RecruitmentRequestCreate = BB.HrRecruitmentRequestCreate;
    public const string RecruitmentRequestSubmit = BB.HrRecruitmentRequestSubmit;
    public const string RecruitmentRequestApprove = BB.HrRecruitmentRequestApprove;
    public const string RecruitmentRequestManage = BB.HrRecruitmentRequestManage;

    public const string OnboardingView = BB.HrOnboardingView;
    public const string OnboardingManage = BB.HrOnboardingManage;
    public const string OnboardingTaskComplete = BB.HrOnboardingTaskComplete;
    public const string OnboardingTaskManage = BB.HrOnboardingTaskManage;

    public const string WorkflowView = BB.HrWorkflowView;
    public const string WorkflowManage = BB.HrWorkflowManage;
    public const string WorkflowExecute = BB.HrWorkflowExecute;
    public const string WorkflowApprove = BB.HrWorkflowApprove;

    public const string OrganizationView = BB.HrOrganizationView;
    public const string OrganizationManage = BB.HrOrganizationManage;
    public const string WorkflowOverride = BB.HrWorkflowOverride;

    public const string LeaveTypeView = BB.HrLeaveTypeView;
    public const string LeaveTypeManage = BB.HrLeaveTypeManage;
    public const string LeavePolicySettingsView = BB.HrLeavePolicySettingsView;
    public const string LeavePolicySettingsManage = BB.HrLeavePolicySettingsManage;
    public const string OvertimeRuleView = BB.HrOvertimeRuleView;
    public const string OvertimeRuleManage = BB.HrOvertimeRuleManage;

    public const string ProbationView = BB.HrProbationView;
    public const string ProbationManage = BB.HrProbationManage;
    public const string SeparationView = BB.HrSeparationView;
    public const string SeparationCreate = BB.HrSeparationCreate;
    public const string EmployeeTimelineView = BB.HrEmployeeTimelineView;

    public const string EssProfileView = BB.HrEssProfileView;
    public const string EssLeaveView = BB.HrEssLeaveView;
    public const string EssLeaveRequest = BB.HrEssLeaveRequest;
    public const string EssAttendanceView = BB.HrEssAttendanceView;
    public const string EssOvertimeView = BB.HrEssOvertimeView;
    public const string EssOvertimeCreate = BB.HrEssOvertimeCreate;
    public const string EssPayrollView = BB.HrEssPayrollView;
    public const string EssPayslipView = BB.HrEssPayslipView;

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
