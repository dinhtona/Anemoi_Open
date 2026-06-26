using System;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands.CommandFlow.CommandManyFlow;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Application.RequestHandlers.Commands.EntityFramework.EfCommandMany;
using Anemoi.Contract.Identity.Commands.IdentityCommands.RemoveUsers;
using Anemoi.Contract.Identity.Errors;
using Anemoi.Identity.Application.Abstractions;
using Anemoi.Identity.Domain.Models;
using Serilog;

namespace Anemoi.Identity.Application.Cqrs.Commands.IdentityCommands.RemoveUsers;

public sealed class RemoveUsersHandler(
    ILogger logger,
    ISqlRepository<User> userDbRepository,
    IUnitOfWork unitOfWork,
    IUserSessionRevocationService sessionRevocationService)
    : EfCommandManyVoidHandler<User, RemoveUsersCommand>(userDbRepository, unitOfWork, logger)
{
    protected override ICommandManyFlowBuilderVoid<User> BuildCommand(
        IStartManyCommandVoid<User> fromFlow, RemoveUsersCommand command,
        CancellationToken cancellationToken) => fromFlow
        .UpdateMany(x => command.UserIds.Contains(x.UserId) && x.IsActivated)
        .WithSpecialAction(null)
        .WithCondition(async users =>
        {
            if (users.Count != command.UserIds.Count)
                return IdentityErrorDetail.UserError.NotFound();

            var prepareResult = await sessionRevocationService.PrepareAsync(
                command.UserIds, cancellationToken);
            if (prepareResult.IsT1)
            {
                Logger.Warning("Failed to prepare session revocation for removed users");
            }
            else
            {
                await sessionRevocationService.PublishAsync(prepareResult.AsT0, cancellationToken);
            }

            return None.Value;
        })
        .WithModify(users =>
        {
            users.ForEach(u =>
            {
                u.IsRemoved = true;
                u.Email = $"{u.Email}_old_{DateTime.UtcNow.Ticks}";
                u.PhoneNumber = $"{u.PhoneNumber}_old_{DateTime.UtcNow.Ticks}";
            });
        })
        .WithErrorIfSaveChange(IdentityErrorDetail.UserError.RemoveFailed());
}