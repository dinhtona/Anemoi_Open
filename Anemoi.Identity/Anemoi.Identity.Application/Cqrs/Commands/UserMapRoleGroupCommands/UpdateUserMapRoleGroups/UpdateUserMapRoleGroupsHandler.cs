using System.Collections.Generic;
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
using Anemoi.Identity.Application.Abstractions;
using Anemoi.Identity.Domain.Models;
using Anemoi.Contract.Identity.Commands.UserMapRoleGroupCommands.UpdateUserMapRoleGroups;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Anemoi.Identity.Application.Cqrs.Commands.UserMapRoleGroupCommands.UpdateUserMapRoleGroups;

public sealed class UpdateUserMapRoleGroupsHandler(
    ISqlRepository<UserMapRoleGroup> sqlRepository,
    ISqlRepository<RoleGroup> roleGroupRepository,
    IUnitOfWork unitOfWork,
    IUserSessionRevocationService sessionRevocationService,
    ILogger logger)
    : EfCommandManyVoidHandler<UserMapRoleGroup, UpdateUserMapRoleGroupsCommand>(sqlRepository, unitOfWork,
        logger)
{
    private PreparedSessionRevocation _preparedRevocation;

    protected override ICommandManyFlowBuilderVoid<UserMapRoleGroup> BuildCommand(
        IStartManyCommandVoid<UserMapRoleGroup> fromFlow, UpdateUserMapRoleGroupsCommand command,
        CancellationToken cancellationToken)
        => fromFlow
            .CreateMany(() => Task.FromResult((command.RoleGroupIds ?? []).Select(roleGroupId => new UserMapRoleGroup
                {
                    Id = new UserMapRoleGroupId(IdGenerator.NextGuid()), UserId = command.UserId,
                    RoleGroupId = roleGroupId
                }).ToList()))
            .WithCondition(async _ =>
            {
                var roleGroupIds = command.RoleGroupIds ?? [];
                if (roleGroupIds.Distinct().Count() != roleGroupIds.Count)
                    return IdentityErrorDetail.UserMapRoleGroupError.RoleGroupsRequestDuplicated();

                var validRoleGroupCount = await roleGroupRepository
                    .GetQueryable(roleGroup => roleGroupIds.Contains(roleGroup.Id) &&
                        !roleGroup.RoleGroupClaims.Any(claim =>
                            claim.Key == AuthorizationClaimTypes.WorkspaceId))
                    .CountAsync(cancellationToken);
                if (validRoleGroupCount != roleGroupIds.Count)
                    return IdentityErrorDetail.RoleGroupError.NotFound();

                var existRoleGroups = await SqlRepository
                    .GetQueryable(a => a.UserId == command.UserId)
                    .ToListAsync(cancellationToken);
                await SqlRepository.RemoveManyAsync(existRoleGroups, cancellationToken);

                var prepareResult = await sessionRevocationService.PrepareAsync(
                    [command.UserId], cancellationToken);
                if (prepareResult.IsT1) return prepareResult.AsT1;

                _preparedRevocation = prepareResult.AsT0;
                return None.Value;
            })
            .WithErrorIfSaveChange(IdentityErrorDetail.UserMapRoleGroupError.CreateFailed());

    protected override Task AfterSaveChangesAsync(UpdateUserMapRoleGroupsCommand command,
        List<UserMapRoleGroup> models, CancellationToken cancellationToken) =>
        sessionRevocationService.PublishAsync(_preparedRevocation, cancellationToken);
}
