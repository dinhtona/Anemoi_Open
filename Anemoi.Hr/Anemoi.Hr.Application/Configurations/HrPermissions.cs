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
    public const string PositionView = Anemoi.BuildingBlock.Application.Authorization.Permissions.HrPositionView;
    public const string DashboardView = "hr.dashboard.view";
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
        PositionView,
        DashboardView,
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
        PayrollLock
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
            [PayrollLock] = new(PayrollLock, "Critical")
        };
}

public sealed record SensitivePermissionDefinition(string Code, string RiskLevel);
