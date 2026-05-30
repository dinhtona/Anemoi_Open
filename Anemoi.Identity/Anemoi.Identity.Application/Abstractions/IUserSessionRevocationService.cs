using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Errors;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Contract.Identity.ModelIds;
using OneOf;

namespace Anemoi.Identity.Application.Abstractions;

public sealed record PreparedSessionRevocation(IReadOnlyList<UserId> UserIds, DateTime RevokedAt);

public interface IUserSessionRevocationService
{
    Task<OneOf<PreparedSessionRevocation, ErrorDetail>> PrepareAsync(
        IEnumerable<UserId> userIds, CancellationToken cancellationToken);

    Task PublishAsync(PreparedSessionRevocation revocation, CancellationToken cancellationToken);

    Task<OneOf<None, ErrorDetail>> RevokeAsync(
        IEnumerable<UserId> userIds, CancellationToken cancellationToken);
}
