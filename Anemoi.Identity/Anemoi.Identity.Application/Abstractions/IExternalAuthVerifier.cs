using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Identity.Application.Abstractions;

/// <summary>
/// Verified identity profile returned by an external OAuth provider.
/// Null result means verification failed (invalid/expired token, audience mismatch, ...).
/// </summary>
public sealed record ExternalAuthVerificationResult(
    string Email,
    string FirstName,
    string LastName);

/// <summary>
/// Verifies an external OAuth token against its provider and extracts the user profile.
/// Implementations live in Infrastructure (outbound HTTP); Application only depends on this abstraction.
/// </summary>
public interface IExternalAuthVerifier
{
    bool CanHandle(string provider);

    Task<ExternalAuthVerificationResult?> VerifyAsync(string token, CancellationToken cancellationToken);
}
