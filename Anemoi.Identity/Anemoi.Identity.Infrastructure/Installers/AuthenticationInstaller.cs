using System;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Configurations;
using Anemoi.BuildingBlock.Application.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Anemoi.Identity.Infrastructure.Installers;

public sealed class AuthenticationInstaller : IInstaller
{
    public void InstallerServices(IServiceCollection services, IConfiguration configuration)
    {
        var jwtSetting = configuration.GetSection(nameof(JwtSetting)).Get<JwtSetting>()!;
        services.AddSingleton(jwtSetting);

        var privateKeyPath = JwtSecurity.ResolveKeyPath(jwtSetting.PrivateKeyPath,
            JwtSecurity.DevelopmentPrivateKeyRelativePath);
        var publicKeyPath = JwtSecurity.ResolveKeyPath(jwtSetting.PublicKeyPath,
            JwtSecurity.DevelopmentPublicKeyRelativePath);
        JwtSecurity.EnsureDevelopmentKeyPair(privateKeyPath, publicKeyPath);
        var privateSecurityKey = JwtSecurity.GetPrivateSecurityKey(privateKeyPath);
        var privateSigningCredential = JwtSecurity.GetPrivateSigningCredential(privateSecurityKey);
        services.AddSingleton(privateSigningCredential);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = privateSecurityKey,
            ValidateAudience = true,
            ValidAudience = jwtSetting.Audience,
            ValidateLifetime = true,
            ValidateIssuer = true,
            ValidIssuer = jwtSetting.Issuer,
            ClockSkew = TimeSpan.Zero
        };
        // System-to-system token: no lifetime check but still validates iss/aud
        var tokenValidationParametersForSystem = tokenValidationParameters.Clone();
        tokenValidationParametersForSystem.ValidateLifetime = false;

        // Get token validation from everywhere
        services.AddSingleton(tokenValidationParametersForSystem);
        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(x =>
        {
            x.SaveToken = true;
            x.TokenValidationParameters = tokenValidationParameters;
        });
    }
}
