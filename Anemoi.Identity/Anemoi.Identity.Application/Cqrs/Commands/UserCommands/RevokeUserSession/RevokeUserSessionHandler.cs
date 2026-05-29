using System;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Contract.Identity.Commands.UserCommands.RevokeUserSession;
using Anemoi.Contract.Identity.Events;
using Anemoi.Identity.Domain.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Identity.Application.Cqrs.Commands.UserCommands.RevokeUserSession;

public sealed class RevokeUserSessionHandler(
    ISqlRepository<User> userRepository,
    ISqlRepository<RefreshToken> refreshTokenRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint)
    : ICommandHandler<RevokeUserSessionCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(
        RevokeUserSessionCommand request, CancellationToken cancellationToken)
    {
        // 1. Get user
        var user = await userRepository.GetFirstByConditionAsync(x => x.UserId == request.UserId, token: cancellationToken);
        if (user is null)
        {
            return new ErrorDetailResponse
            {
                Code = "UserNotFound",
                Messages = new[] { "User not found." }
            };
        }

        // 2. Revoke all refresh tokens
        var tokens = await refreshTokenRepository.GetManyByConditionAsync(x => x.UserId == request.UserId, token: cancellationToken);
        await refreshTokenRepository.RemoveManyAsync(tokens, cancellationToken);

        // 3. Update security stamp to invalidate current JWT
        user.SecurityStamp = IdGenerator.NextGuid().ToString();
        // EF Core tracks this mutation automatically

        // 4. Save Changes
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. Publish event to Gateway to update blacklist immediately
        await publishEndpoint.Publish(new UserTokenRevokedIntegrationEvent
        {
            UserId = request.UserId.Value,
            RevokedAt = DateTime.UtcNow
        }, cancellationToken);

        return None.Value;
    }
}
