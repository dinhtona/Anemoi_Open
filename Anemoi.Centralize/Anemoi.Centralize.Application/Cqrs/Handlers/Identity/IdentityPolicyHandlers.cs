using System.Globalization;
using Anemoi.BuildingBlock.Application.Authorization;
using Anemoi.Centralize.Application.Abstractions;
using Anemoi.Centralize.Application.Cqrs.Requests.Identity;
using Anemoi.Contract.Identity.Queries.RoleQueries.GetRoles;
using Anemoi.Contract.Identity.Responses;
using MediatR;
using Serilog;

namespace Anemoi.Centralize.Application.Cqrs.Handlers.Identity;

public sealed class GetIdentityPolicyRolesHandler(
    IRequestClientService requestClientService,
    PermissionMetadataLocalizer metadataLocalizer,
    ILogger logger)
    : IRequestHandler<GetIdentityPolicyRolesRequest, List<UserRoleResponse>>
{
    public async Task<List<UserRoleResponse>> Handle(GetIdentityPolicyRolesRequest request,
        CancellationToken cancellationToken)
    {
        var page = await requestClientService.GetPaginationResponseAsync<GetUserRolesQuery, UserRoleResponse>(
            new GetUserRolesQuery
            {
                PageIndex = 1,
                PageSize = int.MaxValue
            }, cancellationToken);

        var culture = CultureInfo.CurrentUICulture;

        return page.Items
            .Where(role => role.Name != SystemRoles.Administrator)
            .Select(role =>
            {
                var metadata = metadataLocalizer.Localize(role.Name, culture);
                if (metadata.HasMissingKey)
                {
                    logger.Warning(
                        "Missing permission localization resource for role {RoleName} (culture {Culture}). Raw key surfaced instead of a fabricated display name.",
                        role.Name, culture.Name);
                }

                role.Group = metadata.Group;
                role.Description = metadata.Description;
                return role;
            })
            .ToList();
    }
}
