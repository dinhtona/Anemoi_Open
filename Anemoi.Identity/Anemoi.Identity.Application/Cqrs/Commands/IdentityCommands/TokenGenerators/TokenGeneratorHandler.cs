using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Configurations;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Errors;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.Identity.Application.Abstractions;
using Anemoi.Identity.Application.IdentityResults;
using Anemoi.Identity.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OneOf;

namespace Anemoi.Identity.Application.Cqrs.Commands.IdentityCommands.TokenGenerators;

public sealed class TokenGeneratorHandler(
    SigningCredentials signingCredentials,
    JwtSetting jwtSetting,
    IUserRepository userRepository,
    IUserClaimRepository userClaimRepository,
    ISqlRepository<IdentityPolicy> identityPolicyRepository)
    : ICommandHandler<TokenGeneratorCommand, OneOf<IdentitySuccess, ErrorDetail>>
{
    public async Task<OneOf<IdentitySuccess, ErrorDetail>> Handle(TokenGeneratorCommand request,
        CancellationToken cancellationToken)
    {
        var user = request.User;
        var tokenHandler = new JwtSecurityTokenHandler();
        var claimsIdentity = new ClaimsIdentity();
        var issuedAt = DateTimeOffset.UtcNow;
        claimsIdentity.AddClaim(new Claim(JwtRegisteredClaimNames.Jti, IdGenerator.NextGuid().ToString()));
        claimsIdentity.AddClaim(new Claim(JwtRegisteredClaimNames.Iat,
            issuedAt.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));
        claimsIdentity.AddClaim(new Claim(AuthorizationClaimTypes.TokenIssuedAtUtcTicks,
            issuedAt.UtcTicks.ToString(), ClaimValueTypes.Integer64));
        claimsIdentity.AddClaim(new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName ?? string.Empty));
        claimsIdentity.AddClaim(new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName ?? string.Empty));
        claimsIdentity.AddClaim(new Claim("id", user.UserId.ToString()));
        var userRoles = (await userRepository.GetEffectiveRolesAsync(user)).ToList();
        claimsIdentity.AddClaims(userRoles.Select(role => new Claim(ClaimsIdentity.DefaultRoleClaimType, role)));

        // Dynamically add policy claims based on user roles
        // Include IdentityPolicyMapRoles -> Role so EF Core can translate
        // the mr.Role.Name sub-query to SQL without a NullReferenceException.
        var policyClaims = await identityPolicyRepository.GetManyByConditionAsync(
            p => p.Key != AuthorizationClaimTypes.ApplicationPolicyAgency &&
                p.IdentityPolicyMapRoles.Any(mr => userRoles.Contains(mr.Role.Name)),
            db => db.Include(p => p.IdentityPolicyMapRoles).ThenInclude(mr => mr.Role),
            token: cancellationToken);
        foreach (var policy in policyClaims)
        {
            claimsIdentity.AddClaim(new Claim(policy.Key, policy.Value));
        }

        var claims = await userClaimRepository
            .GetUserClaimsAsync(user.UserId, cancellationToken);
        claimsIdentity.AddClaims(claims.Where(claim =>
            !AuthorizationClaimTypes.ReservedApplicationPolicyClaims.Contains(claim.Type)));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimsIdentity,
            Expires = DateTime.UtcNow.Add(jwtSetting.TokenLifetime),
            Issuer = jwtSetting.Issuer,
            Audience = jwtSetting.Audience,
            SigningCredentials = signingCredentials,
        };
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var token = tokenHandler.WriteToken(securityToken);
        return new IdentitySuccess
        {
            UserToken = token,
            TokenExpiryTime = tokenDescriptor.Expires,
            RefreshTokenExpiryTime = DateTime.UtcNow.Add(jwtSetting.RefreshTokenLifetime),
            JwtId = securityToken.Id,
            CreationDate = DateTime.UtcNow,
            UserId = user.UserId
        };
    }
};
