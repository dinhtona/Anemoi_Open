using System.Collections.Generic;

namespace Anemoi.BuildingBlock.Application.Helpers;

public static class AuthorizationClaimTypes
{
    public const string TokenIssuedAtUtcTicks = "tokenIssuedAtUtcTicks";
    public const string ApplicationPolicyInternal = "applicationPolicyInternal";
    public const string ApplicationPolicyAgency = "applicationPolicyAgency";
    public const string ApplicationPolicyUser = "applicationPolicyUser";
    public const string WorkspaceId = "workspaceId";

    public static readonly IReadOnlyList<string> ReservedApplicationPolicyClaims =
    [
        ApplicationPolicyInternal,
        ApplicationPolicyAgency,
        ApplicationPolicyUser
    ];
}
