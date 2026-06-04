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
        LeaveTransactionView
    ];

    public static readonly IReadOnlyDictionary<string, SensitivePermissionDefinition> SensitivePermissions =
        new Dictionary<string, SensitivePermissionDefinition>
        {
            [LeaveBalanceAdjust] = new(LeaveBalanceAdjust, "High"),
            [LeaveRequestForceApprove] = new(LeaveRequestForceApprove, "High"),
            [LeaveRequestForceCancel] = new(LeaveRequestForceCancel, "High")
        };
}

public sealed record SensitivePermissionDefinition(string Code, string RiskLevel);
