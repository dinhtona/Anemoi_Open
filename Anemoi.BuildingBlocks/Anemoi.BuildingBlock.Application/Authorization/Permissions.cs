using System.Collections.Generic;

namespace Anemoi.BuildingBlock.Application.Authorization;

public static class Permissions
{
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

    public static readonly IReadOnlyList<string> All =
    [
        UserRead,
        UserManage,
        RoleRead,
        RoleManage,
        SeedGeneratorRead,
        SeedGeneratorManage,
        SeedExecutionRun,
        SeedExecutionManualWrite,
        SeedExecutionDeleteRow,
        SeedExecutionLogClear,
        EnvironmentRead,
        EnvironmentStartStop,
        EnvironmentSftpRead,
        EnvironmentSftpManage,
        EnvironmentMockRouteRead,
        EnvironmentMockRouteManage
    ];
}
