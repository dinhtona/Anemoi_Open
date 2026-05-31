using System;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands.CommandFlow.CommandOneFlow;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Application.RequestHandlers.Commands.EntityFramework.EfCommandOne;
using Anemoi.Contract.Identity.Commands.IdentityCommands.UserLogout;
using Anemoi.Contract.Identity.Errors;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Identity.Application.Abstractions;
using Anemoi.Identity.Domain.Models;
using Serilog;

namespace Anemoi.Identity.Application.Cqrs.Commands.IdentityCommands.UserLogout;

public sealed class UserLogoutHandler(
    ILogger logger,
    ISqlRepository<RefreshToken> refreshTokenRepository,
    IUnitOfWork unitOfWork,
    IUserSessionRevocationService sessionRevocationService)
    : EfCommandOneVoidHandler<RefreshToken, UserLogoutCommand>(refreshTokenRepository, unitOfWork, logger)
{
    protected override ICommandOneFlowBuilderVoid<RefreshToken> BuildCommand(
        IStartOneCommandVoid<RefreshToken> fromFlow, UserLogoutCommand command,
        CancellationToken cancellationToken) => fromFlow
        .RemoveOne(x => x.UserToken == command.Token)
        .WithSpecialAction(null)
        .WithCondition(async refreshToken =>
        {
            // NOTE: signInManager.SignOutAsync() is intentionally NOT called here.
            // It writes authentication cookies and requires HttpContext, which is not
            // available in MassTransit consumers. For stateless JWT auth, cookie sign-out
            // is irrelevant — token invalidation is handled by deleting the RefreshToken
            // from DB (done by RemoveOne above) and revoking the access token in Redis below.

            // Best-effort: revoke access token in distributed cache so the JWT cannot
            // be reused before it naturally expires.
            // Wrapped in try-catch so logout always succeeds even if RabbitMQ is temporarily down.
            if (refreshToken?.UserId is UserId userId)
            {
                try
                {
                    await sessionRevocationService.PublishAsync(
                        new PreparedSessionRevocation([userId], System.DateTime.UtcNow),
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.Warning(ex,
                        "Failed to publish token revocation event for user {UserId}. " +
                        "Access token will expire naturally. Logout still succeeded.", userId);
                }
            }
            return None.Value;
        }).WithErrorIfNull(IdentityErrorDetail.TokenError.RefreshTokenNotFound())
        .WithErrorIfSaveChange(IdentityErrorDetail.IdentityError.UserLogOutFailed());
}