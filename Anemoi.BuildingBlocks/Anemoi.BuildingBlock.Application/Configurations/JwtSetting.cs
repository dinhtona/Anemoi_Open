using System;

namespace Anemoi.BuildingBlock.Application.Configurations;

public sealed class JwtSetting
{
    public string PrivateKeyPath { get; set; } = string.Empty;
    public string PublicKeyPath { get; set; } = string.Empty;
    /// <summary>Token issuer (e.g. "anemoi-identity"). Used in JWT 'iss' claim.</summary>
    public string Issuer { get; set; } = string.Empty;
    /// <summary>Token audience (e.g. "anemoi-services"). Used in JWT 'aud' claim.</summary>
    public string Audience { get; set; } = string.Empty;
    public TimeSpan TokenLifetime { get; set; }
    public TimeSpan RefreshTokenLifetime { get; set; }
}
