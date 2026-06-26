using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Anemoi.BuildingBlock.Application.Helpers;
using FluentAssertions;
using Xunit;

namespace Anemoi.Hr.Test.Application.Auth;

public class JwtTokenSizeTests
{
    [Fact]
    public void Jwt_role_group_claim_type_is_role_group_not_ClaimTypes_Role()
    {
        AuthorizationClaimTypes.RoleGroup.Should().Be("role_group");
        AuthorizationClaimTypes.RoleGroup.Should().NotBe(ClaimTypes.Role);
    }

    [Fact]
    public void Jwt_with_role_group_codes_is_below_4096_bytes()
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new(AuthorizationClaimTypes.TokenIssuedAtUtcTicks, DateTimeOffset.UtcNow.Ticks.ToString()),
            new(JwtRegisteredClaimNames.GivenName, "Admin"),
            new(JwtRegisteredClaimNames.FamilyName, "User"),
            new("id", Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, "admin@anemoi.test"),
            new(AuthorizationClaimTypes.RoleGroup, "administrator"),
            new(AuthorizationClaimTypes.RoleGroup, "hr"),
            new(AuthorizationClaimTypes.RoleGroup, "employee"),
        };

        var identity = new ClaimsIdentity(claims);
        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateJwtSecurityToken(subject: identity);
        var jwtString = handler.WriteToken(token);
        jwtString.Length.Should().BeLessThan(4096,
            $"JWT must fit within browser cookie limit. Actual size: {jwtString.Length} bytes");
    }

    [Fact]
    public void Jwt_does_not_contain_permission_claims()
    {
        var claims = new List<Claim>
        {
            new(AuthorizationClaimTypes.RoleGroup, "hr"),
        };
        var identity = new ClaimsIdentity(claims);
        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateJwtSecurityToken(subject: identity);

        token.Payload.Should().NotContainKey("role",
            "JWT must not contain 'role' claims (the old permission-in-JWT mechanism)");
        token.Payload.Should().ContainKey(AuthorizationClaimTypes.RoleGroup,
            "JWT must contain 'role_group' claims");
    }
}
