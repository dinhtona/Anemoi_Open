using System.Collections.Generic;
using System.Linq;

namespace Anemoi.BuildingBlock.Application.Authorization;

public static class Permissions
{
    public sealed record Definition(string Key, string GroupKey, string DescriptionKey);

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
        new(EnvironmentMockRouteManage, "PermissionGroupEnvironments", "PermissionDescriptionEnvironmentMockRouteManage")
    ];

    public static readonly IReadOnlyList<string> All = Definitions.Select(x => x.Key).ToList();

    public static Definition Find(string key) => Definitions.FirstOrDefault(x => x.Key == key);
}
