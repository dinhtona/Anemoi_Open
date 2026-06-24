using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Authorization;
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

public static class EmployeeRoleSeeder
{
    public const string EmployeeRoleName = "Employee";

    public static readonly string[] EmployeePermissions =
    [
        "hr.ess.profile.view",
        "hr.ess.leave.view",
        "hr.ess.leave.request",
        "hr.ess.attendance.view",
        "hr.ess.overtime.view",
        "hr.ess.overtime.create",
        "hr.ess.payroll.view",
        "hr.ess.payslip.view",
        "hr.ess.onboarding.view",
        "hr.ess.onboarding.task.complete",
        "notification.view",
        "notification.preference.manage",
        "notification.action.execute"
    ];

    public static async Task SeedDefaultRoleGroupsAsync(IServiceScope serviceScope)
    {
        var roleRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<Role>>();
        var roleGroupRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<RoleGroup>>();
        var unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger>();

        var allRoles = await roleRepository.GetQueryable().AsNoTracking().ToListAsync();
        var roleByName = allRoles.ToDictionary(r => r.Name!);

        var expectedRoleIds = new List<RoleId>();
        foreach (var perm in EmployeePermissions)
        {
            if (roleByName.TryGetValue(perm, out var role))
                expectedRoleIds.Add(role.RoleId);
            else
                logger.Warning("[SeedData] Role '{Permission}' not found for Employee role group", perm);
        }

        if (expectedRoleIds.Count == 0)
        {
            logger.Warning("[SeedData] No roles found for Employee role group, skipping");
            return;
        }

        var existing = await roleGroupRepository
            .GetFirstByConditionAsync(
                x => x.Name == EmployeeRoleName && x.IsDefault,
                query => query.Include(rg => rg.RoleGroupMapRoles));

        if (existing is not null)
        {
            var existingRoleIds = existing.RoleGroupMapRoles
                .Select(m => m.RoleId)
                .ToHashSet();
            var missingRoleIds = expectedRoleIds
                .Where(rid => !existingRoleIds.Contains(rid))
                .ToList();

            if (missingRoleIds.Count == 0)
            {
                logger.Information("[SeedData] Employee role group is up to date");
                return;
            }

            foreach (var rid in missingRoleIds)
            {
                existing.RoleGroupMapRoles.Add(new RoleGroupMapRole
                {
                    Id = new RoleGroupMapUserRoleId(IdGenerator.NextGuid()),
                    RoleGroupId = existing.Id,
                    RoleId = rid
                });
            }

            await unitOfWork.SaveChangesAsync();
            logger.Information(
                "[SeedData] Synced {Count} missing permissions to Employee role group",
                missingRoleIds.Count);
            return;
        }

        var roleGroup = new RoleGroup
        {
            Id = new RoleGroupId(IdGenerator.NextGuid()),
            Name = EmployeeRoleName,
            Description = "Base employee self-service role",
            IsDefault = true,
            CreatedTime = DateTime.UtcNow,
            RoleGroupMapRoles = expectedRoleIds.Select(rid => new RoleGroupMapRole
            {
                Id = new RoleGroupMapUserRoleId(IdGenerator.NextGuid()),
                RoleId = rid
            }).ToList(),
            RoleGroupClaims = []
        };

        await roleGroupRepository.CreateOneAsync(roleGroup);
        await unitOfWork.SaveChangesAsync();
        logger.Information("[SeedData] Seeded Employee role group with {Count} permissions", expectedRoleIds.Count);
    }
}

public static class SeedData
{
    public static async Task RegisterAdministratorAsync(IServiceScope serviceScope)
    {
        const string administrator = SystemRoles.Administrator;
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

    public static async Task RegisterDevTestUsersAsync(IServiceScope serviceScope)
    {
        var userDbRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<User>>();
        var userRepository = serviceScope.ServiceProvider.GetRequiredService<IUserRepository>();
        var mediator = serviceScope.ServiceProvider.GetRequiredService<IMediator>();
        var config = serviceScope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger>();
        var seedUsers = config.GetSection(nameof(SeedUserData)).Get<SeedUserData>()?.DevTestUsers ?? [];

        foreach (var user in seedUsers)
        {
            var email = user.UserName?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email)) continue;
            if (await userDbRepository.ExistByConditionAsync(x => x.Email == email && x.IsActivated))
                continue;

            var result = await mediator.Send(new CreateUserCommand
            {
                Email = email,
                Password = user.Password,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActivated = true
            });
            if (result.IsT1)
            {
                logger.Warning("[SeedData] Failed to create dev test user {@Email}: {@Error}",
                    email, result.AsT1);
            }
        }

        // Assign roles to dev test users for journey testing
        await AssignDevTestUserRolesAsync(serviceScope);
    }

    private static async Task AssignDevTestUserRolesAsync(IServiceScope serviceScope)
    {
        var userDbRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<User>>();
        var userRepository = serviceScope.ServiceProvider.GetRequiredService<IUserRepository>();
        var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger>();

        var allUsers = await userDbRepository.GetQueryable().ToListAsync();
        var userByEmail = allUsers.ToDictionary(u => u.Email?.Trim().ToLowerInvariant() ?? "");

        var roleAssignments = new Dictionary<string, string[]>
        {
            // Employee role — ESS access only
            ["minh.tran@anemoi.test"] =
            [
                "hr.ess.profile.view", "hr.ess.leave.view", "hr.ess.leave.request",
                "hr.ess.attendance.view", "hr.ess.overtime.view", "hr.ess.overtime.create",
                "hr.ess.payroll.view", "hr.ess.payslip.view"
            ],
            ["an.pham@anemoi.test"] =
            [
                "hr.ess.profile.view", "hr.ess.leave.view", "hr.ess.leave.request",
                "hr.ess.attendance.view", "hr.ess.overtime.view", "hr.ess.overtime.create",
                "hr.ess.payroll.view", "hr.ess.payslip.view"
            ],
            // Manager role — ESS + approval permissions
            ["linh.nguyen@anemoi.test"] =
            [
                "hr.ess.profile.view", "hr.ess.leave.view", "hr.ess.leave.request",
                "hr.ess.attendance.view", "hr.ess.overtime.view", "hr.ess.overtime.create",
                "hr.ess.payroll.view", "hr.ess.payslip.view",
                "hr.leave.request.approve", "hr.overtime.approve",
                "hr.employee.view", "hr.dashboard.view"
            ],
            // HR Manager role — HR operational permissions
            ["mai.le@anemoi.test"] =
            [
                "hr.ess.profile.view", "hr.ess.leave.view", "hr.ess.leave.request",
                "hr.ess.attendance.view", "hr.ess.overtime.view", "hr.ess.overtime.create",
                "hr.ess.payroll.view", "hr.ess.payslip.view",
                "hr.employee.view",
                "hr.contract.create", "hr.contract.view",
                "hr.attendance.create", "hr.attendance.lock", "hr.attendance.update", "hr.attendance.view",
                "hr.payroll.view", "hr.payroll.calculate", "hr.payroll.approve", "hr.payroll.lock",
                "hr.payslip.document.generate", "hr.payslip.document.view", "hr.payslip.email.send",
                "hr.dashboard.view", "hr.analytics.view",
                "hr.leave.request.approve", "hr.leave.request.force_approve",
                "hr.overtime.approve",
                "hr.overtime.manage"
            ],
            // Recruiter role
            ["khoa.do@anemoi.test"] =
            [
                "hr.ess.profile.view", "hr.ess.leave.view", "hr.ess.leave.request",
                "hr.ess.attendance.view", "hr.ess.overtime.view", "hr.ess.overtime.create",
                "hr.ess.payroll.view", "hr.ess.payslip.view",
                "hr.recruitment.view", "hr.recruitment.request.create",
                "hr.recruitment.request.submit", "hr.recruitment.request.approve",
                "hr.recruitment.request.manage",
                "hr.recruitment.manage", "hr.recruitment.interview", "hr.recruitment.hire",
                "hr.dashboard.view"
            ]
        };

        foreach (var (email, roles) in roleAssignments)
        {
            if (!userByEmail.TryGetValue(email, out var user))
            {
                logger.Warning("[SeedData] Dev test user {Email} not found for role assignment", email);
                continue;
            }

            var directRoles = await userRepository.GetDirectRolesAsync(user);
            var rolesToAdd = roles.Where(r => !directRoles.Contains(r)).ToList();
            if (rolesToAdd.Count > 0)
            {
                var addResult = await userRepository.AddToRolesAsync(user, rolesToAdd);
                if (addResult.IsT1)
                    logger.Warning("[SeedData] Failed to assign roles to {Email}: {Error}",
                        email, addResult.AsT1.Message);
                else
                    logger.Information("[SeedData] Assigned {Count} roles to {Email}", rolesToAdd.Count, email);
            }
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
        const string administrator = SystemRoles.Administrator;
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
        var devTestEmails = config.GetSection(nameof(SeedUserData)).Get<SeedUserData>()?.DevTestUsers
            ?.Select(u => u.UserName?.Trim().ToLowerInvariant())
            .ToHashSet() ?? [];
        var protectedEmails = new HashSet<string>(seedAdminEmails, StringComparer.OrdinalIgnoreCase);
        foreach (var devEmail in devTestEmails)
            protectedEmails.Add(devEmail);

        var affectedUserIds = new HashSet<UserId>();

        var users = await userDbRepository.GetQueryable().ToListAsync();
        foreach (var user in users)
        {
            if (protectedEmails.Contains(user.Email?.Trim().ToLowerInvariant() ?? ""))
                continue;

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
            .SelectMany(x => x.Roles)
            .Concat(Permissions.All)
            .Append(EmployeeRoleSeeder.EmployeeRoleName)
            .Distinct();
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

        var roleByName = systemRoles.ToDictionary(role => role.Name!);
        var isChanged = false;
        foreach (var applicationPolicy in defaultApplicationPolicies.ApplicationPolicies)
        {
            var roleNames = applicationPolicy.Roles
                .Concat(applicationPolicy.Key == AuthorizationClaimTypes.ApplicationPolicyInternal
                    ? Permissions.All
                    : [])
                .Distinct()
                .ToList();
            var existingPolicy = await identityPolicyRepository.GetFirstByConditionAsync(
                x => x.Key == applicationPolicy.Key && x.Value == applicationPolicy.Value,
                query => query.Include(policy => policy.IdentityPolicyMapRoles));
            if (existingPolicy is { })
            {
                existingPolicy.IdentityPolicyMapRoles ??= [];
                var mappedRoleIds = existingPolicy.IdentityPolicyMapRoles
                    .Select(mapping => mapping.UserRoleId)
                    .ToHashSet();
                foreach (var roleName in roleNames)
                {
                    var roleId = roleByName[roleName].RoleId;
                    if (mappedRoleIds.Contains(roleId)) continue;

                    existingPolicy.IdentityPolicyMapRoles.Add(new IdentityPolicyMapRole
                    {
                        Id = new IdentityPolicyMapRoleId(IdGenerator.NextGuid()),
                        IdentityPolicyId = existingPolicy.Id,
                        UserRoleId = roleId
                    });
                    isChanged = true;
                }
                continue;
            }

            isChanged = true;
            var identityPolicyMapRoles = roleNames
                .Select(a => new IdentityPolicyMapRole
                {
                    Id = new IdentityPolicyMapRoleId(IdGenerator.NextGuid()),
                    UserRoleId = roleByName[a].RoleId
                });
            var newOne = new IdentityPolicy
            {
                Id = new IdentityPolicyId(IdGenerator.NextGuid()), Key = applicationPolicy.Key,
                Value = applicationPolicy.Value, IdentityPolicyMapRoles = identityPolicyMapRoles.ToList()
            };
            await identityPolicyRepository.CreateOneAsync(newOne);
        }

        if (isChanged) await unitOfWork.SaveChangesAsync();
    }
}
