using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.Identity.Application.Abstractions;

namespace Anemoi.Identity.Infrastructure.Services;

public sealed class MicrosoftExternalAuthVerifier(IHttpClientFactory httpClientFactory)
    : IExternalAuthVerifier
{
    public bool CanHandle(string provider) =>
        provider.Equals("Microsoft", StringComparison.OrdinalIgnoreCase);

    public async Task<ExternalAuthVerificationResult?> VerifyAsync(
        string token, CancellationToken cancellationToken)
    {
        var httpClient = httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        const string msUrl = "https://graph.microsoft.com/v1.0/me";
        using var response = await httpClient.GetAsync(msUrl, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        string? email = null;
        if (root.TryGetProperty("mail", out var mailProp) && mailProp.ValueKind != JsonValueKind.Null)
            email = mailProp.GetString();

        if (string.IsNullOrEmpty(email) && root.TryGetProperty("userPrincipalName", out var upnProp))
            email = upnProp.GetString();

        if (string.IsNullOrEmpty(email))
            return null;

        var firstName = root.TryGetProperty("givenName", out var givenNameProp)
            ? givenNameProp.GetString() ?? string.Empty
            : string.Empty;
        var lastName = root.TryGetProperty("surname", out var surnameProp)
            ? surnameProp.GetString() ?? string.Empty
            : string.Empty;

        return new ExternalAuthVerificationResult(email, firstName, lastName);
    }
}
