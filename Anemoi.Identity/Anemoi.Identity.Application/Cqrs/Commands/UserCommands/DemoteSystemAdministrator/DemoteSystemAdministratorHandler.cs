using System;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Authorization;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Contract.Identity.Commands.UserCommands.DemoteSystemAdministrator;
using Anemoi.Contract.Identity.Errors;
using Anemoi.Identity.Application.Abstractions;
using Anemoi.Identity.Domain.Models;
using Microsoft.EntityFrameworkCore;
using OneOf;
using Serilog;

namespace Anemoi.Identity.Application.Cqrs.Commands.UserCommands.DemoteSystemAdministrator;

public sealed class DemoteSystemAdministratorHandler(
    ISqlRepository<User> userDbRepository,
    IUserRepository userRepository,
    IUserSessionRevocationService sessionRevocationService,
    IUserIdGetter userIdGetter,
    ILogger logger)
    : ICommandHandler<DemoteSystemAdministratorCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(
        DemoteSystemAdministratorCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId.Value == Guid.Parse(userIdGetter.UserId))
            return IdentityErrorDetail.UserError.CannotDemoteSelf().ToErrorDetailResponse();

        var user = await userDbRepository.GetQueryable(x =>
                x.UserId == request.UserId && x.IsActivated && !x.IsRemoved)
            .FirstOrDefaultAsync(cancellationToken);
        if (user is null) return IdentityErrorDetail.UserError.NotFound().ToErrorDetailResponse();

        var roles = await userRepository.GetDirectRolesAsync(user);
        if (!roles.Contains(SystemRoles.Administrator))
            return IdentityErrorDetail.UserError.NotAdministrator().ToErrorDetailResponse();

        var administrators = await userRepository.GetUsersInRoleAsync(SystemRoles.Administrator);
        if (administrators.Count <= 1)
            return IdentityErrorDetail.UserError.CannotDemoteLastAdministrator().ToErrorDetailResponse();

        var removeResult = await userRepository.RemoveFromRolesAsync(user, [SystemRoles.Administrator]);
        if (removeResult.IsT1) return IdentityErrorDetail.RoleError.RemoveRolesError().ToErrorDetailResponse();

        var revokeResult = await sessionRevocationService.RevokeAsync([request.UserId], cancellationToken);
        if (revokeResult.IsT1) return revokeResult.AsT1.ToErrorDetailResponse();

        logger.Warning("[SecurityAudit] Demoted user {UserId} from system administrator", request.UserId);
        return None.Value;
    }
}
