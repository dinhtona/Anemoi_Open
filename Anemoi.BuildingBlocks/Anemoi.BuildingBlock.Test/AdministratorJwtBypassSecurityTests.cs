using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Anemoi.BuildingBlock.Application.Configurations;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Authorization;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Centralize.Api.Controllers.MasterData;
using Anemoi.Centralize.Infrastructure.Installers;
using Anemoi.Contract.MasterData.Commands.SampleAddressCommands.CreateSampleAddress;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using OneOf;
using Xunit;

namespace Anemoi.BuildingBlock.Test;

public sealed class AdministratorJwtBypassSecurityTests
{
    private const string Issuer = "anemoi-identity";
    private const string Audience = "anemoi-services";

    [Fact]
    public async Task ValidSignedToken_WithAdministratorRoleClaim_AndInternalPolicy_AllowsAdminEndpoint()
    {
        await using var host = await CreateHostAsync();
        var token = CreateSignedToken(includeRoleClaim: true, includeAdministratorRoleGroup: false);

        var response = await SendAdminRequestAsync(host.Client, token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ValidSignedToken_WithAdministratorRoleGroupOnly_DoesNotBypassRoleAuthorization()
    {
        await using var host = await CreateHostAsync();
        var token = CreateSignedToken(includeRoleClaim: false, includeAdministratorRoleGroup: true);

        var response = await SendAdminRequestAsync(host.Client, token);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task TamperedToken_AddingAdministratorRoleGroup_WithOldSignature_IsRejected()
    {
        await using var host = await CreateHostAsync();
        var signedEmployeeToken = CreateSignedToken(includeRoleClaim: false, includeAdministratorRoleGroup: false);
        var tamperedToken = TamperPayload(signedEmployeeToken, payload =>
        {
            payload["role_group"] = "administrator";
        });

        var response = await SendAdminRequestAsync(host.Client, tamperedToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UnsignedTokenWithAlgNone_AndAdministratorRoleGroup_IsRejected()
    {
        await using var host = await CreateHostAsync();
        var token = CreateNoneAlgorithmToken();

        var response = await SendAdminRequestAsync(host.Client, token);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WrongSecretSignedToken_WithAdministratorRoleGroup_IsRejected()
    {
        await using var host = await CreateHostAsync();
        var token = CreateTokenSignedWithWrongKey();

        var response = await SendAdminRequestAsync(host.Client, token);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static async Task<TestHostContext> CreateHostAsync()
    {
        var repoRoot = FindRepoRoot();
        var publicKeyPath = Path.GetFullPath(
            Path.Combine(repoRoot, "Anemoi.Centralize", "Anemoi.Centralize.Api", "Certifications", "public.crt"));
        var privateKeyPath = Path.GetFullPath(
            Path.Combine(repoRoot, "Anemoi.Identity", "Anemoi.Identity.WorkerService", "Certifications", "private.key"));

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            [$"{nameof(JwtSetting)}:{nameof(JwtSetting.PublicKeyPath)}"] = publicKeyPath,
            [$"{nameof(JwtSetting)}:{nameof(JwtSetting.Issuer)}"] = Issuer,
            [$"{nameof(JwtSetting)}:{nameof(JwtSetting.Audience)}"] = Audience,
            [$"{nameof(JwtSetting)}:{nameof(JwtSetting.TokenLifetime)}"] = "00:15:00",
        });

        builder.Services.AddControllers().AddApplicationPart(typeof(AdminController).Assembly);
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSingleton<ISender>(CreateSenderStub());

        new AuthenticationInstaller().InstallerServices(builder.Services, builder.Configuration);
        new AuthorizationInstaller().InstallerServices(builder.Services, builder.Configuration);

        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();

        return new TestHostContext(app, app.GetTestClient(), publicKeyPath, privateKeyPath);
    }

    private static ISender CreateSenderStub()
    {
        var sender = NSubstitute.Substitute.For<ISender>();
        sender.Send(Arg.Any<CreateSampleAddressCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<OneOf<None, Anemoi.BuildingBlock.Application.Responses.ErrorDetailResponse>>(None.Value));
        return sender;
    }

    private static async Task<HttpResponseMessage> SendAdminRequestAsync(HttpClient client, string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/masterData/Admin/CreateSampleAddress");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.SendAsync(request);
    }

    private static string CreateSignedToken(bool includeRoleClaim, bool includeAdministratorRoleGroup)
    {
        var repoRoot = FindRepoRoot();
        var privateKeyPath = Path.GetFullPath(
            Path.Combine(repoRoot, "Anemoi.Identity", "Anemoi.Identity.WorkerService", "Certifications", "private.key"));
        var signingCredentials = JwtSecurity.GetPrivateSigningCredential(privateKeyPath);
        var issuedAt = DateTimeOffset.UtcNow;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, issuedAt.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(AuthorizationClaimTypes.TokenIssuedAtUtcTicks, issuedAt.UtcTicks.ToString(), ClaimValueTypes.Integer64),
            new("id", Guid.NewGuid().ToString()),
            new("email", "employee@anemoi.test"),
            new(AuthorizationClaimTypes.ApplicationPolicyInternal, AuthorizationPolicies.Internal),
            new(AuthorizationClaimTypes.RoleGroup, includeAdministratorRoleGroup ? SystemRoles.Administrator : "employee")
        };

        if (includeRoleClaim)
        {
            claims.Add(new Claim(ClaimTypes.Role, SystemRoles.Administrator));
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = Issuer,
            Audience = Audience,
            Expires = issuedAt.AddMinutes(15).UtcDateTime,
            SigningCredentials = signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(tokenHandler.CreateToken(descriptor));
    }

    private static string CreateTokenSignedWithWrongKey()
    {
        using var rsa = RSA.Create(2048);
        var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);
        return CreateTokenWithSigningCredentials(signingCredentials);
    }

    private static string CreateNoneAlgorithmToken()
    {
        var issuedAt = DateTimeOffset.UtcNow;
        var payload = new Dictionary<string, object?>
        {
            [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
            [JwtRegisteredClaimNames.Iat] = issuedAt.ToUnixTimeSeconds(),
            [AuthorizationClaimTypes.TokenIssuedAtUtcTicks] = issuedAt.UtcTicks,
            ["id"] = Guid.NewGuid().ToString(),
            ["email"] = "employee@anemoi.test",
            [AuthorizationClaimTypes.ApplicationPolicyInternal] = AuthorizationPolicies.Internal,
            [AuthorizationClaimTypes.RoleGroup] = SystemRoles.Administrator
        };

        var header = new Dictionary<string, object?>
        {
            ["alg"] = "none",
            ["typ"] = "JWT"
        };

        return $"{Base64UrlEncoder.Encode(JsonSerializer.SerializeToUtf8Bytes(header))}." +
               $"{Base64UrlEncoder.Encode(JsonSerializer.SerializeToUtf8Bytes(payload))}.";
    }

    private static string CreateTokenWithSigningCredentials(SigningCredentials signingCredentials)
    {
        var issuedAt = DateTimeOffset.UtcNow;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, issuedAt.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(AuthorizationClaimTypes.TokenIssuedAtUtcTicks, issuedAt.UtcTicks.ToString(), ClaimValueTypes.Integer64),
            new("id", Guid.NewGuid().ToString()),
            new("email", "employee@anemoi.test"),
            new(AuthorizationClaimTypes.ApplicationPolicyInternal, AuthorizationPolicies.Internal),
            new(AuthorizationClaimTypes.RoleGroup, SystemRoles.Administrator)
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = Issuer,
            Audience = Audience,
            Expires = issuedAt.AddMinutes(15).UtcDateTime,
            SigningCredentials = signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(tokenHandler.CreateToken(descriptor));
    }

    private static string TamperPayload(string token, Action<JsonObject> mutate)
    {
        var parts = token.Split('.');
        if (parts.Length != 3) throw new InvalidOperationException("Invalid JWT format.");

        var payloadJson = Encoding.UTF8.GetString(Base64UrlEncoder.DecodeBytes(parts[1]));
        var payloadNode = JsonNode.Parse(payloadJson)?.AsObject()
            ?? throw new InvalidOperationException("JWT payload is not valid JSON.");

        mutate(payloadNode);

        var tamperedPayload = payloadNode.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = false
        });

        return $"{parts[0]}.{Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(tamperedPayload))}.{parts[2]}";
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "AGENTS.md")))
                return current.FullName;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Unable to locate repository root from test output directory.");
    }

    private sealed record TestHostContext(WebApplication App, HttpClient Client, string PublicKeyPath, string PrivateKeyPath)
        : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await App.DisposeAsync();
        }
    }
}
