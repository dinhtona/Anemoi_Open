using System;
using System.Linq;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Authorization;
using Anemoi.BuildingBlock.Application.Cqrs.Queries.QueryFlow.QueryOneFlow;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.RequestHandlers.Queries.EntityFramework.EfQueryOne;
using Anemoi.Contract.Identity.Errors;
using Anemoi.Contract.Identity.Queries.UserQueries.GetUser;
using Anemoi.Contract.Identity.Responses;
using Anemoi.Identity.Application.Abstractions;
using Anemoi.Identity.Application.Mappings;
using Anemoi.Identity.Domain.Models;
using Microsoft.EntityFrameworkCore;
using OneOf;
using Serilog;

namespace Anemoi.Identity.Application.Cqrs.Queries.UserQueries.GetUser;

public sealed class GetUserHandler(
    ISqlRepository<User> sqlRepository,
    ISqlRepository<UserMapRoleGroup> userMapRoleGroupRepository,
    IUserRepository userRepository,
    IdentityMapper mapper,
    ILogger logger)
    : EfQueryOneHandler<User, GetUserQuery, UserResponse>(sqlRepository, logger)
{
    protected override IQueryOneFlowBuilder<User, UserResponse> BuildQueryFlow(
        IQueryOneFilter<User, UserResponse> fromFlow, GetUserQuery query)
        => fromFlow
            .WithFilter(x => x.UserId == query.Id)
            .WithSpecialAction(x => x)
            .WithErrorIfNull(IdentityErrorDetail.UserError.NotFound());

    protected override async Task<UserResponse> MapToResultAsync(GetUserQuery query, OneOf<User, UserResponse> modelOrResponse)
    {
        if (modelOrResponse.IsT1) return modelOrResponse.AsT1;
        var user = modelOrResponse.AsT0;

        var response = mapper.ToUserResponse(user);

        // Fetch role group mapping
        var roleGroups = await userMapRoleGroupRepository.GetManyByConditionAsync(
            x => x.UserId == user.UserId &&
                !x.RoleGroup.RoleGroupClaims.Any(claim =>
                    claim.Key == AuthorizationClaimTypes.WorkspaceId),
            query => query.Include(x => x.RoleGroup),
            token: default);
        response.RoleGroupIds = roleGroups.Select(rg => rg.RoleGroupId.ToString()).ToList();
        response.RoleGroupNames = roleGroups.Select(rg => rg.RoleGroup.Name).ToList();

        response.DirectRoles = (await userRepository.GetDirectRolesAsync(user)).ToList();
        response.Permissions = (await userRepository.GetEffectiveRolesAsync(user)).ToList();
        response.Roles = response.DirectRoles
            .Where(role => role == SystemRoles.Administrator)
            .Concat(response.RoleGroupNames)
            .Distinct()
            .ToList();

        return response;
    }
}
