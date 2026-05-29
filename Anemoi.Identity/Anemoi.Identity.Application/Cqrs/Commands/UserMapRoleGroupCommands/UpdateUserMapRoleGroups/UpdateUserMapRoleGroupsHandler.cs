using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands.CommandFlow.CommandManyFlow;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Infrastructure.RequestHandlers.Commands.EntityFramework.EfCommandMany;
using Anemoi.Contract.Identity.Errors;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Contract.Identity.Events;
using Anemoi.Identity.Domain.Models;
using Anemoi.Contract.Identity.Commands.UserMapRoleGroupCommands.UpdateUserMapRoleGroups;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Anemoi.Identity.Application.Cqrs.Commands.UserMapRoleGroupCommands.UpdateUserMapRoleGroups;

public sealed class UpdateUserMapRoleGroupsHandler(
    ISqlRepository<UserMapRoleGroup> sqlRepository,
    ISqlRepository<User> userRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    ILogger logger)
    : EfCommandManyVoidHandler<UserMapRoleGroup, UpdateUserMapRoleGroupsCommand>(sqlRepository, unitOfWork,
        logger)
{
    protected override ICommandManyFlowBuilderVoid<UserMapRoleGroup> BuildCommand(
        IStartManyCommandVoid<UserMapRoleGroup> fromFlow, UpdateUserMapRoleGroupsCommand command,
        CancellationToken cancellationToken)
        => fromFlow
            .CreateMany(async () =>
            {
                // Remove all existing roles for this user
                var existRoleGroups = await SqlRepository
                    .GetQueryable(a => a.UserId == command.UserId)
                    .ToListAsync(cancellationToken);
                await SqlRepository.RemoveManyAsync(existRoleGroups, cancellationToken);

                // Update Security Stamp to force token refresh
                var user = await userRepository.GetFirstByConditionAsync(x => x.UserId == command.UserId, token: cancellationToken);
                if (user is not null)
                {
                    user.SecurityStamp = IdGenerator.NextGuid().ToString();
                    // EF Core tracks this mutation automatically
                }

                // Publish revocation event so the API Gateway rejects current JWT and forces RefreshToken
                await publishEndpoint.Publish(new UserTokenRevokedIntegrationEvent
                {
                    UserId = command.UserId.Value,
                    RevokedAt = DateTime.UtcNow
                }, cancellationToken);

                return command.RoleGroupIds.Select(roleGroupId => new UserMapRoleGroup
                {
                    Id = new UserMapRoleGroupId(IdGenerator.NextGuid()), UserId = command.UserId,
                    RoleGroupId = roleGroupId
                }).ToList();
            })
            .WithCondition(_ => None.Value)
            .WithErrorIfSaveChange(IdentityErrorDetail.UserMapRoleGroupError.CreateFailed());
}