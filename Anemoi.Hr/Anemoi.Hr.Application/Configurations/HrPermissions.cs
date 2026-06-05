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
        GradeChange
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
            [GradeChange] = new(GradeChange, "High")
        };
}

public sealed record SensitivePermissionDefinition(string Code, string RiskLevel);
