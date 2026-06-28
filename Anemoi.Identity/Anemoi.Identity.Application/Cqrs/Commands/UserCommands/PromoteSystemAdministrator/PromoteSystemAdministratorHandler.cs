using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Authorization;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Contract.Identity.Commands.UserCommands.PromoteSystemAdministrator;
using Anemoi.Contract.Identity.Errors;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Identity.Application.Abstractions;
using Anemoi.Identity.Domain.Models;
using Lambda.Identity.Application.SeedData;
using Microsoft.EntityFrameworkCore;
using OneOf;
using Serilog;

namespace Anemoi.Identity.Application.Cqrs.Commands.UserCommands.PromoteSystemAdministrator;

public sealed class PromoteSystemAdministratorHandler(
    ISqlRepository<User> userDbRepository,
    IUserRepository userRepository,
    ISqlRepository<RoleGroup> roleGroupRepository,
    ISqlRepository<UserMapRoleGroup> userMapRoleGroupRepository,
    IUnitOfWork unitOfWork,
    IUserSessionRevocationService sessionRevocationService,
    ILogger logger)
    : ICommandHandler<PromoteSystemAdministratorCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(
        PromoteSystemAdministratorCommand request, CancellationToken cancellationToken)
    {
        var user = await userDbRepository.GetQueryable(x =>
                x.UserId == request.UserId && x.IsActivated && !x.IsRemoved)
            .FirstOrDefaultAsync(cancellationToken);
        if (user is null) return IdentityErrorDetail.UserError.NotFound().ToErrorDetailResponse();

        var roles = await userRepository.GetDirectRolesAsync(user);
        if (roles.Contains(SystemRoles.Administrator))
            return IdentityErrorDetail.UserError.AlreadyAdministrator().ToErrorDetailResponse();

        var addResult = await userRepository.AddToRolesAsync(user, [SystemRoles.Administrator]);
        if (addResult.IsT1) return IdentityErrorDetail.RoleError.AddRolesError().ToErrorDetailResponse();

        await EnsureAdminRoleGroupAssignmentAsync(request.UserId, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var revokeResult = await sessionRevocationService.RevokeAsync([request.UserId], cancellationToken);
        if (revokeResult.IsT1) return revokeResult.AsT1.ToErrorDetailResponse();

        logger.Warning("[SecurityAudit] Promoted user {UserId} to system administrator", request.UserId);
        return None.Value;
    }

    private async Task EnsureAdminRoleGroupAssignmentAsync(
        UserId userId, CancellationToken cancellationToken)
    {
        var adminRoleGroup = await roleGroupRepository.GetFirstByConditionAsync(
            x => x.IsDefault && x.Code == SystemRoleProfiles.Admin.Code,
            token: cancellationToken);
        if (adminRoleGroup is null) return;

        var exists = await userMapRoleGroupRepository.ExistByConditionAsync(
            x => x.UserId == userId && x.RoleGroupId == adminRoleGroup.Id,
            cancellationToken);
        if (exists) return;

        await userMapRoleGroupRepository.CreateOneAsync(new UserMapRoleGroup
        {
            Id = new UserMapRoleGroupId(IdGenerator.NextGuid()),
            UserId = userId,
            RoleGroupId = adminRoleGroup.Id
        }, cancellationToken);
    }
}
