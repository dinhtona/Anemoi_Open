using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.Identity.Application.Abstractions;
using Anemoi.Identity.Application.Configurations;

namespace Anemoi.Identity.Infrastructure.Services;

public sealed class GoogleExternalAuthVerifier(
    IHttpClientFactory httpClientFactory,
    ExternalAuthSetting externalAuthSetting)
    : IExternalAuthVerifier
{
    public bool CanHandle(string provider) =>
        provider.Equals("Google", StringComparison.OrdinalIgnoreCase);

    public async Task<ExternalAuthVerificationResult?> VerifyAsync(
        string token, CancellationToken cancellationToken)
    {
        var httpClient = httpClientFactory.CreateClient();

        var googleUrl = $"https://oauth2.googleapis.com/tokeninfo?id_token={token}";
        using var response = await httpClient.GetAsync(googleUrl, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        if (string.IsNullOrWhiteSpace(externalAuthSetting.GoogleClientId) ||
            !root.TryGetProperty("aud", out var audienceProp) ||
            audienceProp.ValueKind != JsonValueKind.String ||
            !string.Equals(audienceProp.GetString(), externalAuthSetting.GoogleClientId,
                StringComparison.Ordinal))
        {
            return null;
        }

        if (!root.TryGetProperty("email", out var emailProp))
            return null;

        var email = emailProp.GetString();
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var firstName = root.TryGetProperty("given_name", out var givenNameProp)
            ? givenNameProp.GetString() ?? string.Empty
            : string.Empty;
        var lastName = root.TryGetProperty("family_name", out var familyNameProp)
            ? familyNameProp.GetString() ?? string.Empty
            : string.Empty;

        return new ExternalAuthVerificationResult(email, firstName, lastName);
    }
}
