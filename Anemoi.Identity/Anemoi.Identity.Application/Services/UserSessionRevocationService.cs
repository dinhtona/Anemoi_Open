using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Errors;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Contract.Identity.Errors;
using Anemoi.Contract.Identity.Events;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Identity.Application.Abstractions;
using Anemoi.Identity.Domain.Models;
using MassTransit;
using OneOf;

namespace Anemoi.Identity.Application.Services;

public sealed class UserSessionRevocationService(
    ISqlRepository<User> userRepository,
    ISqlRepository<RefreshToken> refreshTokenRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint) : IUserSessionRevocationService
{
    public async Task<OneOf<PreparedSessionRevocation, ErrorDetail>> PrepareAsync(
        IEnumerable<UserId> userIds, CancellationToken cancellationToken)
    {
        var ids = userIds.Distinct().ToList();
        var revocation = new PreparedSessionRevocation(ids, DateTime.UtcNow);
        if (ids.Count == 0) return revocation;

        var users = await userRepository.GetManyByConditionAsync(
            user => ids.Contains(user.UserId), token: cancellationToken);
        if (users.Count != ids.Count) return IdentityErrorDetail.UserError.NotFound();

        var tokens = await refreshTokenRepository.GetManyByConditionAsync(
            token => ids.Contains(token.UserId), token: cancellationToken);
        await refreshTokenRepository.RemoveManyAsync(tokens, cancellationToken);

        foreach (var user in users)
        {
            user.SecurityStamp = IdGenerator.NextGuid().ToString();
        }

        return revocation;
    }

    public async Task PublishAsync(PreparedSessionRevocation revocation, CancellationToken cancellationToken)
    {
        foreach (var userId in revocation.UserIds)
        {
            await publishEndpoint.Publish(new UserTokenRevokedIntegrationEvent
            {
                UserId = userId.Value,
                RevokedAt = revocation.RevokedAt
            }, cancellationToken);
        }
    }

    public async Task<OneOf<None, ErrorDetail>> RevokeAsync(
        IEnumerable<UserId> userIds, CancellationToken cancellationToken)
    {
        var prepareResult = await PrepareAsync(userIds, cancellationToken);
        if (prepareResult.IsT1) return prepareResult.AsT1;

        await PublishAsync(prepareResult.AsT0, cancellationToken);
        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1) return IdentityErrorDetail.UserError.UpdateFailed();

        return None.Value;
    }
}
