using System;
using System.Threading;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands.CommandFlow.CommandOneFlow;
using Anemoi.BuildingBlock.Application.RequestHandlers.Commands.EntityFramework.EfCommandOne;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Contract.Identity.Commands.IdentityCommands.UserChangePassword;
using Anemoi.Contract.Identity.Errors;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Contract.Identity.Responses;
using Anemoi.Identity.Application.Abstractions;
using Anemoi.Identity.Domain.Models;
using Serilog;

namespace Anemoi.Identity.Application.Cqrs.Commands.IdentityCommands.UserChangePassword;

public sealed class UserChangePasswordHandler(
    ILogger logger,
    IUserRepository userRepository,
    ISqlRepository<User> userDbRepository,
    IUnitOfWork unitOfWork,
    IUserIdGetter userIdGetter,
    IUserSessionRevocationService sessionRevocationService)
    : EfCommandOneResultHandler<User, UserChangePasswordCommand, UserIdResponse>(
        userDbRepository, unitOfWork, logger)
{
    protected override ICommandOneFlowBuilderResult<User, UserIdResponse> BuildCommand(
        IStartOneCommandResult<User, UserIdResponse> fromFlow,
        UserChangePasswordCommand command, CancellationToken cancellationToken) =>
        fromFlow
            .UpdateOne(x => x.UserId == new UserId(Guid.Parse(userIdGetter.UserId)) && x.IsActivated)
            .WithSpecialAction(null)
            .WithCondition(async user =>
            {
                var changePasswordResult = await userRepository
                    .ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword);
                if (changePasswordResult.IsT1)
                    return IdentityErrorDetail.IdentityError.FailedToChangePassword();

                user.ChangedPasswordTime = DateTime.UtcNow;

                var prepareResult = await sessionRevocationService.PrepareAsync(
                    [user.UserId], cancellationToken);
                if (prepareResult.IsT1)
                {
                    Logger.Warning("Failed to prepare session revocation for user {UserId} after password change",
                        user.UserId);
                }
                else
                {
                    await sessionRevocationService.PublishAsync(prepareResult.AsT0, cancellationToken);
                }

                return None.Value;
            })
            .WithModify(_ => { })
            .WithErrorIfNull(IdentityErrorDetail.UserError.NotFound())
            .WithErrorIfSaveChange(IdentityErrorDetail.IdentityError.FailedToChangePassword())
            .WithResultIfSucceed(user => new UserIdResponse { Id = user.UserId.ToString() });
}