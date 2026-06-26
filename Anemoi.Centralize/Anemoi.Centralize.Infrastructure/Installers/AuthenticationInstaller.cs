using System.Globalization;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Configurations;
using Anemoi.BuildingBlock.Application.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Anemoi.Centralize.Infrastructure.Installers;

public sealed class AuthenticationInstaller : IInstaller
{
    public void InstallerServices(IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection(nameof(JwtSetting)).Get<JwtSetting>();
        
        var publicKeyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, jwtSettings.PublicKeyPath);
        var publicSigningCredential = JwtSecurity.GetPublicSigningCredential(publicKeyPath);
        
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = publicSigningCredential,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ClockSkew = TimeSpan.Zero
        };
        var tokenParameterWithoutExpired = tokenValidationParameters.Clone();
        tokenParameterWithoutExpired.ValidateLifetime = false;
        services.AddSingleton(tokenParameterWithoutExpired);
        
        // Get token validation from everywhere
        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(x =>
        {
            x.SaveToken = true;
            x.TokenValidationParameters = tokenValidationParameters;
            x.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var path = context.HttpContext.Request.Path;
                    if (path.StartsWithSegments("/hubs/notification") &&
                        context.Request.Query.TryGetValue("access_token", out var accessToken))
                    {
                        context.Token = accessToken;
                    }
                    else if (context.Request.Cookies.ContainsKey("access_token"))
                    {
                        context.Token = context.Request.Cookies["access_token"];
                    }
                    return Task.CompletedTask;
                },
                OnTokenValidated = async context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<AuthenticationInstaller>>();
                    var issuedAtClaim = context.Principal?
                        .FindFirst(AuthorizationClaimTypes.TokenIssuedAtUtcTicks)?.Value;
                    if (!long.TryParse(issuedAtClaim, NumberStyles.Integer, CultureInfo.InvariantCulture,
                            out var issuedAtTicks))
                    {
                        context.Fail("Token issuance metadata is missing or invalid.");
                        return;
                    }

                    var userIdClaim = context.Principal?.FindFirst("id")?.Value;
                    if (string.IsNullOrEmpty(userIdClaim))
                    {
                        context.Fail("Token user metadata is missing.");
                        return;
                    }

                    try
                    {
                        var distributedCache = context.HttpContext.RequestServices
                            .GetRequiredService<IDistributedCache>();
                        var revokedAtValue = await distributedCache
                            .GetStringAsync($"revoked_user:{userIdClaim}", context.HttpContext.RequestAborted);
                        if (long.TryParse(revokedAtValue, NumberStyles.Integer, CultureInfo.InvariantCulture,
                                out var revokedAtTicks) && issuedAtTicks <= revokedAtTicks)
                        {
                            context.Fail("Token has been revoked.");
                        }
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(exception,
                            "Unable to validate JWT revocation state for user {UserId}. " +
                            "Allowing token to proceed — token revocation cache is unavailable.",
                            userIdClaim);
                    }
                }
            };
        });
    }
}
