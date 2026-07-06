using System;
using System.Linq;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.Contract.Identity.Events;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Contract.Hr.Events;
using Lambda.Identity.Application.SeedData;
using Anemoi.Identity.Domain.Models;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Anemoi.Identity.Application.Consumers;

public sealed class EmployeeAutoProvisionConsumer(
    UserManager<User> userManager,
    ISqlRepository<RoleGroup> roleGroupRepository,
    ISqlRepository<UserMapRoleGroup> userMapRoleGroupRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    ILogger logger)
    : IConsumer<EmployeeCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<EmployeeCreatedIntegrationEvent> context)
    {
        var message = context.Message;
        var email = message.WorkEmail?.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email))
        {
            logger.Warning(
                "[AutoProvision] Employee {EmployeeId} has no work email, skipping auto-provision",
                message.EmployeeId);
            return;
        }

        var existingUser = await userManager.FindByEmailAsync(email);
        User user;
        if (existingUser is not null)
        {
            user = existingUser;
            logger.Information(
                "[AutoProvision] User {Email} already exists (UserId: {UserId}), assigning role group",
                email, user.Id);
        }
        else
        {
            user = new User
            {
                UserId = new UserId(IdGenerator.NextGuid()),
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                FirstName = message.FullName,
                LastName = "",
                NormalizedUserName = email.ToUpperInvariant(),
                NormalizedEmail = email.ToUpperInvariant(),
                SecurityStamp = IdGenerator.NextGuid().ToString(),
                CreatedTime = DateTime.UtcNow,
                IsActivated = false,
                LockoutEnabled = false
            };
            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                logger.Error(
                    "[AutoProvision] Failed to create user {Email}: {Errors}",
                    email, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                return;
            }
            logger.Information(
                "[AutoProvision] Created user {Email} (UserId: {UserId})",
                email, user.Id);
        }

        var employeeRoleGroup = await roleGroupRepository
            .GetFirstByConditionAsync(x => x.Name == EmployeeRoleSeeder.EmployeeRoleName && x.IsDefault);
        if (employeeRoleGroup is null)
        {
            logger.Warning(
                "[AutoProvision] Employee role group not found, skipping role assignment for {Email}",
                email);
            return;
        }

        var alreadyAssigned = await userMapRoleGroupRepository.ExistByConditionAsync(
            x => x.UserId == user.UserId && x.RoleGroupId == employeeRoleGroup.Id);
        if (alreadyAssigned)
        {
            logger.Information(
                "[AutoProvision] User {Email} already has Employee role group, skipping",
                email);
        }
        else
        {
            var mapping = new UserMapRoleGroup
            {
                Id = new UserMapRoleGroupId(IdGenerator.NextGuid()),
                UserId = user.UserId,
                RoleGroupId = employeeRoleGroup.Id
            };
            await userMapRoleGroupRepository.CreateOneAsync(mapping);
            await unitOfWork.SaveChangesAsync();
            logger.Information(
                "[AutoProvision] Assigned Employee role group to user {Email}",
                email);
        }

        await publishEndpoint.Publish(new UserProvisionedForEmployeeIntegrationEvent(
            message.EmployeeId,
            user.UserId.Value,
            email,
            message.EmployeeCode
        ), context.CancellationToken);

        logger.Information(
            "[AutoProvision] Completed provisioning for employee {EmployeeCode} / user {Email}",
            message.EmployeeCode, email);
    }
}
