using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.Contract.Identity.Commands.UserCommands.CreateUser;
using Anemoi.Contract.Identity.ModelIds;
using MediatR;
using Microsoft.Extensions.Configuration;
using Anemoi.Identity.Application.Abstractions;
using Anemoi.Identity.Application.Configurations;
using Anemoi.Identity.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

// ReSharper disable All

namespace Lambda.Identity.Application.SeedData;

public static class SeedData
{
    public static async Task RegisterAdministratorAsync(IServiceScope serviceScope)
    {
        const string administrator = "Administrator";
        var userRepository = serviceScope.ServiceProvider.GetRequiredService<IUserRepository>();
        var userDbRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<User>>();
        var mediator = serviceScope.ServiceProvider.GetRequiredService<IMediator>();
        var config = serviceScope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger>();
        var seedUserData = config.GetSection(nameof(SeedUserData)).Get<SeedUserData>();
        var users = seedUserData.SupperAdminUsers;
        foreach (var user in users)
        {
            var existUser = await userDbRepository.GetFirstByConditionAsync(x => x.Email == user.UserName);
            if (existUser is { })
            {
                var userRoles = await userRepository.GetDirectRolesAsync(existUser);
                if (!userRoles.Contains(administrator))
                    await userRepository.AddToRolesAsync(existUser, [administrator]);
                continue;
            }

            var newAdminCommand = new CreateUserCommand
            {
                Password = user.Password, FirstName = user.FirstName,
                LastName = user.LastName, Email = user.UserName,
                IsActivated = true
            };
            var newUserResult = await mediator.Send(newAdminCommand);
            if (newUserResult.IsT1)
            {
                logger.Warning("[SeedData] Failed to create admin user {@Email}: {@Error}",
                    user.UserName, newUserResult.AsT1);
                continue;
            }
            var createdUser = await userDbRepository.GetFirstByConditionAsync(x =>
                x.UserId == new UserId(Guid.Parse(newUserResult.AsT0.Id)));
            await userRepository.AddToRolesAsync(createdUser, [administrator]);
        }
    }

    public static async Task RemoveReservedApplicationPolicyClaimsAsync(IServiceScope serviceScope)
    {
        var userClaimRepository = serviceScope.ServiceProvider.GetRequiredService<IUserClaimRepository>();
        var reservedClaimTypes = AuthorizationClaimTypes.ReservedApplicationPolicyClaims.ToList();
        var userIds = await userClaimRepository.GetUserIdsByClaimTypes(reservedClaimTypes);

        foreach (var userId in userIds.Distinct())
        {
            await userClaimRepository.RemoveClaimsAsync(new UserId(userId), reservedClaimTypes,
                CancellationToken.None);
        }
    }

    public static async Task NormalizeAuthorizationAssignmentsAsync(IServiceScope serviceScope)
    {
        const string administrator = "Administrator";
        var config = serviceScope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger>();
        var userRepository = serviceScope.ServiceProvider.GetRequiredService<IUserRepository>();
        var userDbRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<User>>();
        var roleGroupMapRoleRepository = serviceScope.ServiceProvider
            .GetRequiredService<ISqlRepository<RoleGroupMapRole>>();
        var userMapRoleGroupRepository = serviceScope.ServiceProvider
            .GetRequiredService<ISqlRepository<UserMapRoleGroup>>();
        var unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var sessionRevocationService = serviceScope.ServiceProvider
            .GetRequiredService<IUserSessionRevocationService>();

        var seedUsers = config.GetSection(nameof(SeedUserData)).Get<SeedUserData>()?.SupperAdminUsers ?? [];
        var seedAdminEmails = seedUsers.Select(user => user.UserName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var affectedUserIds = new HashSet<UserId>();

        var users = await userDbRepository.GetQueryable().ToListAsync();
        foreach (var user in users)
        {
            var directRoles = await userRepository.GetDirectRolesAsync(user);
            var rolesToRemove = directRoles.Where(role =>
                    role != administrator || !seedAdminEmails.Contains(user.Email))
                .ToList();
            if (rolesToRemove.Count == 0) continue;

            var removeResult = await userRepository.RemoveFromRolesAsync(user, rolesToRemove);
            if (removeResult.IsT1)
            {
                logger.Warning("[SeedData] Failed to remove obsolete direct roles from {Email}: {Error}",
                    user.Email, removeResult.AsT1.Message);
                continue;
            }

            affectedUserIds.Add(user.UserId);
        }

        var invalidSystemAdministratorMappings = await roleGroupMapRoleRepository.GetQueryable(mapping =>
                mapping.Role.Name == administrator &&
                !mapping.RoleGroup.RoleGroupClaims.Any(claim =>
                    claim.Key == AuthorizationClaimTypes.WorkspaceId))
            .ToListAsync();
        if (invalidSystemAdministratorMappings.Count > 0)
        {
            var affectedByMappings = await userMapRoleGroupRepository.GetQueryable(mapping =>
                    mapping.RoleGroup.RoleGroupMapRoles.Any(role => role.Role.Name == administrator) &&
                    !mapping.RoleGroup.RoleGroupClaims.Any(claim =>
                        claim.Key == AuthorizationClaimTypes.WorkspaceId))
                .Select(mapping => mapping.UserId)
                .Distinct()
                .ToListAsync();
            affectedUserIds.UnionWith(affectedByMappings);
            await roleGroupMapRoleRepository.RemoveManyAsync(invalidSystemAdministratorMappings);
        }

        if (affectedUserIds.Count > 0)
        {
            var revokeResult = await sessionRevocationService.RevokeAsync(affectedUserIds, CancellationToken.None);
            if (revokeResult.IsT1)
                logger.Warning("[SeedData] Failed to revoke normalized authorization sessions: {Error}",
                    revokeResult.AsT1.Code);
            return;
        }

        await unitOfWork.SaveChangesAsync();
    }

    public static async Task SeedRolesAsync(IServiceScope serviceScope)
    {
        var roleRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<Role>>();
        var unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var defaultApplicationPolicies = serviceScope.ServiceProvider.GetRequiredService<DefaultApplicationPolices>();

        var roles = defaultApplicationPolicies.ApplicationPolicies
            .SelectMany(x => x.Roles).Distinct();
        var isChangeed = false;
        foreach (var role in roles)
        {
            if (await roleRepository.ExistByConditionAsync(x => role == x.Name)) continue;
            var roleUser = new Role { Name = role, RoleId = new RoleId(IdGenerator.NextGuid()) };
            isChangeed = true;
            await roleRepository.CreateOneAsync(roleUser);
        }

        if (isChangeed) await unitOfWork.SaveChangesAsync();
    }

    public static async Task SeedApplicationPoliciesAsync(IServiceScope serviceScope)
    {
        var identityPolicyRepository = serviceScope.ServiceProvider
            .GetRequiredService<ISqlRepository<IdentityPolicy>>();
        var unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var roleRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<Role>>();

        var defaultApplicationPolicies = serviceScope.ServiceProvider.GetRequiredService<DefaultApplicationPolices>();
        var systemRoles = await roleRepository.GetQueryable().AsNoTracking().ToListAsync();

        var roles = defaultApplicationPolicies.ApplicationPolicies
            .SelectMany(x => x.Roles).Distinct();
        var isChangeed = false;
        foreach (var applicationPolicy in defaultApplicationPolicies.ApplicationPolicies)
        {
            var existOne = await identityPolicyRepository.ExistByConditionAsync(x =>
                x.Key == applicationPolicy.Key && x.Value == applicationPolicy.Value);
            if (existOne) continue;
            isChangeed = true;
            var identityPolicyMapRoles = applicationPolicy.Roles
                .Select(a => new IdentityPolicyMapRole
                {
                    Id = new IdentityPolicyMapRoleId(IdGenerator.NextGuid()),
                    UserRoleId = systemRoles.FirstOrDefault(x => x.Name == a)?.RoleId
                });
            var newOne = new IdentityPolicy
            {
                Id = new IdentityPolicyId(IdGenerator.NextGuid()), Key = applicationPolicy.Key,
                Value = applicationPolicy.Value, IdentityPolicyMapRoles = identityPolicyMapRoles.ToList()
            };
            await identityPolicyRepository.CreateOneAsync(newOne);
        }

        if (isChangeed) await unitOfWork.SaveChangesAsync();
    }
}
