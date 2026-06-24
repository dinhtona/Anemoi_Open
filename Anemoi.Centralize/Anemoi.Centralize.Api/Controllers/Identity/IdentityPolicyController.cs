using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Authorization;
using Anemoi.BuildingBlock.Application.Resources;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Contract.Identity.Queries.IdentityPolicyQueries.GetIdentityPolicies;
using Anemoi.Contract.Identity.Queries.RoleQueries.GetRoles;
using Anemoi.Contract.Identity.Responses;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Anemoi.Centralize.Api.Controllers.Identity;

[Route("api/identity/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public class IdentityPolicyController(
    ISender sender,
    IStringLocalizer<SharedResource> localizer) : ControllerBase
{
    /// <summary>
    /// GetIdentityPolicyRoles
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.Internal)]
    [HasPermission(Permissions.RoleRead)]
    [ProducesResponseType(typeof(List<UserRoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetIdentityPolicyRoles(CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetUserRolesQuery
        {
            PageIndex = 1,
            PageSize = int.MaxValue
        }, cancellationToken);
        var roles = res.Items
            .Where(role => role.Name != SystemRoles.Administrator)
            .Select(role =>
            {
                var permission = Permissions.Find(role.Name);
                role.Group = Localize(permission?.GroupKey ?? "PermissionGroupOther", "Other");
                role.Description = permission is null
                    ? Localize("PermissionDescriptionUnmapped", role.Name, role.Name)
                    : Localize(permission.DescriptionKey, ToDisplayName(permission.Key));
                return role;
            });
        return Ok(roles);
    }

    private string Localize(string key, string fallback)
    {
        var localized = localizer[key];
        return localized.ResourceNotFound ? fallback : localized.Value;
    }

    private string Localize(string key, object argument, string fallback)
    {
        var localized = localizer[key, argument];
        return localized.ResourceNotFound ? fallback : localized.Value;
    }

    private static string ToDisplayName(string permissionCode) =>
        string.Join(" ", permissionCode
            .Replace('_', '.')
            .Split('.', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));

    /// <summary>
    /// GetIdentityPolicies
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.Internal)]
    [HasPermission(Permissions.RoleRead)]
    [ProducesResponseType(typeof(CollectionResponse<IdentityPolicyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetIdentityPolicies(CancellationToken cancellationToken)
    {
        var res = await sender
            .Send(new GetIdentityPoliciesQuery(), cancellationToken);
        return Ok(res);
    }

}
