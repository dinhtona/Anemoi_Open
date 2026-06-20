using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Authorization;
using Anemoi.BuildingBlock.Application.Configurations;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Identity.Commands.IdentityCommands.GenerateCustomToken;
using Anemoi.Contract.Identity.Errors;
using Anemoi.Contract.Identity.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using OneOf;

namespace Anemoi.Identity.Application.Cqrs.Commands.IdentityCommands.GenerateCustomToken;

public sealed class GenerateCustomTokenHandler(
    JwtSetting jwtSetting,
    SigningCredentials signingCredentials,
    IHttpContextAccessor httpContextAccessor)
    : ICommandHandler<GenerateCustomTokenCommand, OneOf<CustomTokenResponse, ErrorDetailResponse>>
{
    private static readonly HashSet<string> RestrictedClaimPrefixes =
        ["role", "permission", "applicationPolicy"];

    public async Task<OneOf<CustomTokenResponse, ErrorDetailResponse>> Handle(GenerateCustomTokenCommand request,
        CancellationToken cancellationToken)
    {
        await Task.Yield();

        var roles = httpContextAccessor.HttpContext?.GetUserRoles() ?? [];
        if (!roles.Contains(SystemRoles.Administrator))
            return IdentityErrorDetail.UserError.NotAdministrator().ToErrorDetailResponse();

        var blocked = request.ClaimsRequest
            .Where(c => RestrictedClaimPrefixes.Any(p =>
                c.ClaimType.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        if (blocked.Count != 0)
            return IdentityErrorDetail.TokenError.InvalidToken().ToErrorDetailResponse();

        var tokenHandler = new JwtSecurityTokenHandler();
        var claimsIdentity = new ClaimsIdentity();
        claimsIdentity.AddClaims(request.ClaimsRequest.Select(a => new Claim(a.ClaimType, a.ClaimValue)));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimsIdentity,
            Expires = DateTime.UtcNow.Add(request.ExpiredIn ?? jwtSetting.TokenLifetime),
            SigningCredentials = signingCredentials,
        };
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var token = tokenHandler.WriteToken(securityToken);
        return new CustomTokenResponse { Token = token };
    }
}