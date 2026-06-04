namespace Anemoi.Hr.Application.Configurations;

public static class HrPermissions
{
    public const string LeavePolicyView = "hr.leave.policy.view";
    public const string LeavePolicyCreate = "hr.leave.policy.create";
    public const string LeavePolicyUpdate = "hr.leave.policy.update";
    public const string LeaveBalanceView = "hr.leave.balance.view";
    public const string LeaveBalanceAdjust = "hr.leave.balance.adjust";
    public const string LeaveRequestView = "hr.leave.request.view";
    public const string LeaveRequestCreate = "hr.leave.request.create";
    public const string LeaveRequestApprove = "hr.leave.request.approve";
    public const string LeaveRequestCancel = "hr.leave.request.cancel";
    public const string LeaveRequestForceApprove = "hr.leave.request.force_approve";
    public const string LeaveRequestForceCancel = "hr.leave.request.force_cancel";
    public const string LeaveTransactionView = "hr.leave.transaction.view";

    public static readonly IReadOnlyDictionary<string, SensitivePermissionDefinition> SensitivePermissions =
        new Dictionary<string, SensitivePermissionDefinition>
        {
            [LeaveBalanceAdjust] = new(LeaveBalanceAdjust, "High"),
            [LeaveRequestForceApprove] = new(LeaveRequestForceApprove, "High"),
            [LeaveRequestForceCancel] = new(LeaveRequestForceCancel, "High")
        };
}

public sealed record SensitivePermissionDefinition(string Code, string RiskLevel);
