using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Authorization;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.Contract.Identity.Commands.UserCommands.CreateUser;
using Anemoi.Contract.Identity.Events;
using Anemoi.Contract.Identity.ModelIds;
using MassTransit;
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
        Permissions.HrEssProfileView,
        Permissions.HrEssLeaveView,
        Permissions.HrEssLeaveRequest,
        Permissions.HrEssAttendanceView,
        Permissions.HrEssOvertimeView,
        Permissions.HrEssOvertimeCreate,
        Permissions.HrEssPayrollView,
        Permissions.HrEssPayslipView,
        Permissions.HrOnboardingView,
        Permissions.HrOnboardingTaskComplete,
        Permissions.NotificationView,
        Permissions.NotificationManage,
        Permissions.NotificationPreferenceManage,
        Permissions.NotificationActionExecute,
        Permissions.NotificationArchive
    ];

    private static readonly IReadOnlyCollection<SystemRoleProfile> DefaultProfiles =
    [
        SystemRoleProfiles.Employee,
        SystemRoleProfiles.Hr,
        SystemRoleProfiles.Recruiter,
        SystemRoleProfiles.WorkflowAdmin,
        SystemRoleProfiles.Admin
    ];

    public static async Task SeedDefaultRoleGroupsAsync(IServiceScope serviceScope)
    {
        await SystemRoleGroupSeeder.SeedDefaultRoleGroupsAsync(serviceScope, DefaultProfiles);
    }
}

public sealed record SystemRoleProfile(string Code, string Name, string Description, IReadOnlyCollection<string> Permissions);

public static class SystemRoleProfiles
{
    public static readonly SystemRoleProfile Employee = new(
        "employee",
        EmployeeRoleSeeder.EmployeeRoleName,
        "Base employee self-service role",
        EmployeeRoleSeeder.EmployeePermissions);

    public static readonly SystemRoleProfile Hr = new(
        "hr",
        "HR",
        "HR operations role",
        EmployeeRoleSeeder.EmployeePermissions.Concat([
            Permissions.HrEmployeeView,
            Permissions.HrEmployeeCreate,
            Permissions.HrEmployeeUpdate,
            Permissions.HrEmployeeActivate,
            Permissions.HrEmployeeResume,
            Permissions.HrEmployeeArchive,
            Permissions.HrEmployeeManagerChange,
            Permissions.HrEmployeeDocumentView,
            Permissions.HrEmployeeDocumentManage,
            Permissions.HrEmployeeAssetView,
            Permissions.HrEmployeeAssetManage,
            Permissions.HrEmployeeNoteView,
            Permissions.HrEmployeeTransferView,
            Permissions.HrEmployeeTimelineView,
            Permissions.HrPromotionView,
            Permissions.HrPositionChangeView,
            Permissions.HrGradeChangeView,
            Permissions.HrDepartmentView,
            Permissions.HrDepartmentManage,
            Permissions.HrPositionView,
            Permissions.HrPositionManage,
            Permissions.HrDashboardView,
            Permissions.HrAnalyticsView,
            Permissions.HrContractView,
            Permissions.HrContractCreate,
            Permissions.HrSalaryGradeView,
            Permissions.HrAllowanceTypeView,
            Permissions.HrEmployeeAllowanceView,
            Permissions.HrCompensationDashboardView,
            Permissions.HrPayrollView,
            Permissions.HrPayrollReportingView,
            Permissions.HrPayslipDocumentView,
            Permissions.HrAttendanceView,
            Permissions.HrAttendanceCreate,
            Permissions.HrAttendanceUpdate,
            Permissions.HrAttendanceLock,
            Permissions.HrShiftView,
            Permissions.HrShiftManage,
            Permissions.HrShiftAssign,
            Permissions.HrShiftCancel,
            Permissions.HrOvertimeView,
            Permissions.HrOvertimeRequest,
            Permissions.HrOvertimeManage,
            Permissions.HrOvertimeApprove,
            Permissions.HrCalendarView,
            Permissions.HrCalendarManage,
            Permissions.HrTaxView,
            Permissions.HrTaxCalculate,
            Permissions.HrInsuranceView,
            Permissions.HrInsuranceCalculate,
            Permissions.HrInsuranceReport,
            Permissions.HrRecruitmentView,
            Permissions.HrRecruitmentInterview,
            Permissions.HrRecruitmentAnalytics,
            Permissions.HrRecruitmentRequestView,
            Permissions.HrRecruitmentRequestCreate,
            Permissions.HrWorkflowView,
            Permissions.HrWorkflowManage,
            Permissions.HrWorkflowExecute,
            Permissions.HrOrganizationView,
            Permissions.HrWorkflowOverride,
            Permissions.HrLeavePolicyView,
            Permissions.HrLeavePolicyCreate,
            Permissions.HrLeavePolicyUpdate,
            Permissions.HrLeaveBalanceView,
            Permissions.HrLeaveRequestView,
            Permissions.HrLeaveRequestCreate,
            Permissions.HrLeaveRequestApprove,
            Permissions.HrLeaveRequestCancel,
            Permissions.HrLeaveTransactionView,
            Permissions.HrLeaveTypeView,
            Permissions.HrLeaveTypeManage,
            Permissions.HrLeavePolicySettingsView,
            Permissions.HrLeavePolicySettingsManage,
            Permissions.HrOvertimeRuleView,
            Permissions.HrOvertimeRuleManage,
            Permissions.HrProbationView,
            Permissions.HrProbationManage,
            Permissions.HrSeparationView,
            Permissions.HrSeparationCreate
        ]).Distinct().ToArray());

    public static readonly SystemRoleProfile Recruiter = new(
        "recruiter",
        "Recruiter",
        "Recruitment operations role",
        EmployeeRoleSeeder.EmployeePermissions.Concat([
            Permissions.HrRecruitmentView,
            Permissions.HrRecruitmentRequestCreate,
            Permissions.HrRecruitmentRequestSubmit,
            Permissions.HrRecruitmentRequestManage,
            Permissions.HrRecruitmentManage,
            Permissions.HrRecruitmentInterview,
            Permissions.HrRecruitmentHire
        ]).Distinct().ToArray());

    public static readonly SystemRoleProfile WorkflowAdmin = new(
        "workflow_admin",
        "WorkflowAdmin",
        "Workflow configuration role",
        EmployeeRoleSeeder.EmployeePermissions.Concat([
            Permissions.HrWorkflowView,
            Permissions.HrWorkflowManage,
            Permissions.HrWorkflowExecute
        ]).Distinct().ToArray());

    public static readonly SystemRoleProfile Admin = new(
        "administrator",
        "Admin",
        "Business administration role",
        Permissions.All.ToArray());
}

public static class DevTestRoleAssignments
{
    public static readonly IReadOnlyDictionary<string, string[]> RoleGroupsByEmail =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["linh.nguyen@anemoi.test"] = [SystemRoleProfiles.Employee.Code],
            ["minh.tran@anemoi.test"] = [SystemRoleProfiles.Employee.Code],
            ["an.pham@anemoi.test"] = [SystemRoleProfiles.Employee.Code],
            ["mai.le@anemoi.test"] = [SystemRoleProfiles.Employee.Code, SystemRoleProfiles.Hr.Code],
            ["khoa.do@anemoi.test"] = [SystemRoleProfiles.Employee.Code, SystemRoleProfiles.Hr.Code]
        };
}

internal static class SystemRoleGroupSeeder
{
    public static async Task SeedDefaultRoleGroupsAsync(
        IServiceScope serviceScope,
        IReadOnlyCollection<SystemRoleProfile> profiles)
    {
        var roleRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<Role>>();
        var roleGroupRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<RoleGroup>>();
        var unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger>();

        var allRoles = await roleRepository.GetQueryable().AsNoTracking().ToListAsync();
        var roleByName = allRoles.ToDictionary(r => r.Name!, StringComparer.OrdinalIgnoreCase);

        foreach (var profile in profiles)
        {
            var expectedRoleIds = new List<RoleId>();
            foreach (var perm in profile.Permissions)
            {
                if (roleByName.TryGetValue(perm, out var role))
                    expectedRoleIds.Add(role.RoleId);
                else
                    logger.Warning("[SeedData] Role '{Permission}' not found for {RoleGroup} role group",
                        perm, profile.Name);
            }

            if (expectedRoleIds.Count == 0)
            {
                logger.Warning("[SeedData] No roles found for {RoleGroup} role group, skipping", profile.Name);
                continue;
            }

            var existing = await roleGroupRepository
                .GetFirstByConditionAsync(
                    x => x.Code == profile.Code && x.IsDefault,
                    query => query.Include(rg => rg.RoleGroupMapRoles));

            if (existing is not null)
            {
                existing.Name = profile.Name;
                existing.Description = profile.Description;
                var existingRoleIds = existing.RoleGroupMapRoles
                    .Select(m => m.RoleId)
                    .ToHashSet();
                var missingRoleIds = expectedRoleIds
                    .Where(rid => !existingRoleIds.Contains(rid))
                    .ToList();

                foreach (var rid in missingRoleIds)
                {
                    existing.RoleGroupMapRoles.Add(new RoleGroupMapRole
                    {
                        Id = new RoleGroupMapUserRoleId(IdGenerator.NextGuid()),
                        RoleGroupId = existing.Id,
                        RoleId = rid
                    });
                }

                continue;
            }

            var roleGroupId = new RoleGroupId(IdGenerator.NextGuid());
            var roleGroup = new RoleGroup
            {
                Id = roleGroupId,
                Code = profile.Code,
                Name = profile.Name,
                Description = profile.Description,
                IsDefault = true,
                CreatedTime = DateTime.UtcNow,
                RoleGroupMapRoles = expectedRoleIds.Select(rid => new RoleGroupMapRole
                {
                    Id = new RoleGroupMapUserRoleId(IdGenerator.NextGuid()),
                    RoleGroupId = roleGroupId,
                    RoleId = rid
                }).ToList(),
                RoleGroupClaims = []
            };

            await roleGroupRepository.CreateOneAsync(roleGroup);
        }

        await unitOfWork.SaveChangesAsync();
        logger.Information("[SeedData] Seeded/synced {Count} default system role groups", profiles.Count);
    }
}

public static class SeedData
{
    public static async Task RegisterAdministratorAsync(IServiceScope serviceScope)
    {
        const string administrator = SystemRoles.Administrator;
        var userRepository = serviceScope.ServiceProvider.GetRequiredService<IUserRepository>();
        var userDbRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<User>>();
        var roleGroupRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<RoleGroup>>();
        var userMapRoleGroupRepository = serviceScope.ServiceProvider
            .GetRequiredService<ISqlRepository<UserMapRoleGroup>>();
        var unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var mediator = serviceScope.ServiceProvider.GetRequiredService<IMediator>();
        var config = serviceScope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger>();
        var seedUserData = config.GetSection(nameof(SeedUserData)).Get<SeedUserData>();
        var users = seedUserData.SupperAdminUsers;
        var hasMappingChanges = false;
        foreach (var user in users)
        {
            var existUser = await userDbRepository.GetFirstByConditionAsync(x => x.Email == user.UserName);
            if (existUser is { })
            {
                var userRoles = await userRepository.GetDirectRolesAsync(existUser);
                if (!userRoles.Contains(administrator))
                    await userRepository.AddToRolesAsync(existUser, [administrator]);
                hasMappingChanges |= await EnsureSystemRoleGroupAssignmentAsync(
                    existUser,
                    roleGroupRepository,
                    userMapRoleGroupRepository,
                    logger);
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
            hasMappingChanges |= await EnsureSystemRoleGroupAssignmentAsync(
                createdUser,
                roleGroupRepository,
                userMapRoleGroupRepository,
                logger);
        }

        if (hasMappingChanges)
        await unitOfWork.SaveChangesAsync();
    }

    private static readonly Dictionary<string, (Guid EmployeeId, string EmployeeCode)> DevTestEmployeeIdsByEmail = new(StringComparer.OrdinalIgnoreCase)
    {
        ["linh.nguyen@anemoi.test"] = (Guid.Parse("30000000-0000-0000-0000-000000000001"), "DEV-ENG-001"),
        ["minh.tran@anemoi.test"]   = (Guid.Parse("30000000-0000-0000-0000-000000000002"), "DEV-ENG-002"),
        ["an.pham@anemoi.test"]     = (Guid.Parse("30000000-0000-0000-0000-000000000003"), "DEV-ENG-003"),
        ["mai.le@anemoi.test"]      = (Guid.Parse("30000000-0000-0000-0000-000000000004"), "DEV-HR-001"),
        ["khoa.do@anemoi.test"]     = (Guid.Parse("30000000-0000-0000-0000-000000000005"), "DEV-HR-002"),
        ["admin@anemoi.com"]        = (Guid.Parse("30000000-0000-0000-0000-000000000006"), "DEV-ADMIN-001"),
    };

    private static async Task ProvisionDevTestUsersToEmployeesAsync(IServiceScope serviceScope)
    {
        var userDbRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<User>>();
        var publishEndpoint = serviceScope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger>();

        var users = await userDbRepository.GetQueryable()
            .Where(x => x.IsActivated && x.Email != null)
            .ToListAsync();

        foreach (var user in users)
        {
            var email = user.Email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email)) continue;
            if (!DevTestEmployeeIdsByEmail.TryGetValue(email, out var mapping)) continue;

            await publishEndpoint.Publish(
                new UserProvisionedForEmployeeIntegrationEvent(
                    mapping.EmployeeId, user.UserId.Value, user.Email!, mapping.EmployeeCode));

            logger.Information(
                "[SeedData] Provisioned employee {EmployeeId} ({EmployeeCode}) for user {Email} ({UserId})",
                mapping.EmployeeId, mapping.EmployeeCode, email, user.UserId);
        }
    }

    private static async Task<bool> EnsureSystemRoleGroupAssignmentAsync(
        User? user,
        ISqlRepository<RoleGroup> roleGroupRepository,
        ISqlRepository<UserMapRoleGroup> userMapRoleGroupRepository,
        ILogger logger)
    {
        if (user is null)
            return false;

        var administratorRoleGroup = await roleGroupRepository.GetFirstByConditionAsync(
            x => x.IsDefault && x.Code == SystemRoleProfiles.Admin.Code);
        if (administratorRoleGroup is null)
        {
            logger.Warning("[SeedData] Administrator role group with code {RoleGroupCode} was not found",
                SystemRoleProfiles.Admin.Code);
            return false;
        }

        var mappingExists = await userMapRoleGroupRepository.ExistByConditionAsync(x =>
            x.UserId == user.UserId && x.RoleGroupId == administratorRoleGroup.Id);
        if (mappingExists)
            return false;

        await userMapRoleGroupRepository.CreateOneAsync(new UserMapRoleGroup
        {
            Id = new UserMapRoleGroupId(IdGenerator.NextGuid()),
            UserId = user.UserId,
            RoleGroupId = administratorRoleGroup.Id
        });
        return true;
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

        // Provision: link Identity users to HR employees by email
        await ProvisionDevTestUsersToEmployeesAsync(serviceScope);
    }

    private static async Task AssignDevTestUserRolesAsync(IServiceScope serviceScope)
    {
        var userDbRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<User>>();
        var userRepository = serviceScope.ServiceProvider.GetRequiredService<IUserRepository>();
        var roleGroupRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<RoleGroup>>();
        var userMapRoleGroupRepository = serviceScope.ServiceProvider.GetRequiredService<ISqlRepository<UserMapRoleGroup>>();
        var unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger>();

        var allUsers = await userDbRepository.GetQueryable().ToListAsync();
        var userByEmail = allUsers.ToDictionary(u => u.Email?.Trim().ToLowerInvariant() ?? "");
        var roleGroups = await roleGroupRepository.GetQueryable(x => x.IsDefault && x.Code != null)
            .ToDictionaryAsync(x => x.Code!, StringComparer.OrdinalIgnoreCase);

        foreach (var (email, roleGroupCodes) in DevTestRoleAssignments.RoleGroupsByEmail)
        {
            if (!userByEmail.TryGetValue(email, out var user))
            {
                logger.Warning("[SeedData] Dev test user {Email} not found for role assignment", email);
                continue;
            }

            var directRoles = await userRepository.GetDirectRolesAsync(user);
            var obsoleteDirectRoles = directRoles
                .Where(role => role != SystemRoles.Administrator)
                .ToList();
            if (obsoleteDirectRoles.Count > 0)
            {
                var removeResult = await userRepository.RemoveFromRolesAsync(user, obsoleteDirectRoles);
                if (removeResult.IsT1)
                    logger.Warning("[SeedData] Failed to remove obsolete direct dev roles from {Email}: {Error}",
                        email, removeResult.AsT1.Message);
            }

            foreach (var roleGroupCode in roleGroupCodes)
            {
                if (!roleGroups.TryGetValue(roleGroupCode, out var roleGroup))
                {
                    logger.Warning("[SeedData] Role group {RoleGroupCode} not found for {Email}", roleGroupCode, email);
                    continue;
                }

                var exists = await userMapRoleGroupRepository.ExistByConditionAsync(x =>
                    x.UserId == user.UserId && x.RoleGroupId == roleGroup.Id);
                if (exists) continue;

                await userMapRoleGroupRepository.CreateOneAsync(new UserMapRoleGroup
                {
                    Id = new UserMapRoleGroupId(IdGenerator.NextGuid()),
                    UserId = user.UserId,
                    RoleGroupId = roleGroup.Id
                });
            }
        }

        await unitOfWork.SaveChangesAsync();
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

        var roleByName = systemRoles.ToDictionary(role => role.Name!, StringComparer.OrdinalIgnoreCase);
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
