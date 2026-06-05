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
        new(HrPositionView, "PermissionGroupHrPosition", "PermissionDescriptionHrPositionView")
    ];

    public static readonly IReadOnlyList<string> All = Definitions.Select(x => x.Key).ToList();

    public static Definition Find(string key) => Definitions.FirstOrDefault(x => x.Key == key);
}
