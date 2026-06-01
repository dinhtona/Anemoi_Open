using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.Contract.Identity.Commands.RefreshTokenCommands.ExternalLogin;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Contract.Identity.Responses;
using Anemoi.Identity.Application.Configurations;
using Anemoi.Identity.Application.Cqrs.Commands.IdentityCommands.TokenGenerators;
using Anemoi.Identity.Domain.Models;
using MediatR;
using OneOf;

namespace Anemoi.Identity.Application.Cqrs.Commands.RefreshTokenCommands.ExternalLogin;

public sealed class ExternalLoginHandler(
    IHttpClientFactory httpClientFactory,
    ISqlRepository<User> userRepository,
    ISqlRepository<RefreshToken> refreshTokenRepository,
    IUnitOfWork unitOfWork,
    ISender sender,
    ExternalAuthSetting externalAuthSetting)
    : ICommandHandler<ExternalLoginCommand, OneOf<AuthenticationSuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<AuthenticationSuccessResponse, ErrorDetailResponse>> Handle(ExternalLoginCommand request,
        CancellationToken cancellationToken)
    {
        string email = null;
        string firstName = "";
        string lastName = "";

        try
        {
            var httpClient = httpClientFactory.CreateClient();

            if (request.Provider.Equals("Google", StringComparison.OrdinalIgnoreCase))
            {
                var googleUrl = $"https://oauth2.googleapis.com/tokeninfo?id_token={request.Token}";
                var response = await httpClient.GetAsync(googleUrl, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return CreateError("ExternalAuthFailed");
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                if (string.IsNullOrWhiteSpace(externalAuthSetting.GoogleClientId) ||
                    !root.TryGetProperty("aud", out var audienceProp) ||
                    audienceProp.ValueKind != JsonValueKind.String ||
                    !string.Equals(audienceProp.GetString(), externalAuthSetting.GoogleClientId,
                        StringComparison.Ordinal))
                {
                    return CreateError("ExternalAuthFailed");
                }

                if (!root.TryGetProperty("email", out var emailProp))
                {
                    return CreateError("ExternalAuthFailed");
                }

                email = emailProp.GetString();
                if (root.TryGetProperty("given_name", out var givenNameProp)) firstName = givenNameProp.GetString() ?? "";
                if (root.TryGetProperty("family_name", out var familyNameProp)) lastName = familyNameProp.GetString() ?? "";
            }
            else if (request.Provider.Equals("Microsoft", StringComparison.OrdinalIgnoreCase))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", request.Token);
                var msUrl = "https://graph.microsoft.com/v1.0/me";
                var response = await httpClient.GetAsync(msUrl, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return CreateError("ExternalAuthFailed");
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                if (root.TryGetProperty("mail", out var mailProp) && mailProp.ValueKind != JsonValueKind.Null)
                {
                    email = mailProp.GetString();
                }

                if (string.IsNullOrEmpty(email) && root.TryGetProperty("userPrincipalName", out var upnProp))
                {
                    email = upnProp.GetString();
                }

                if (string.IsNullOrEmpty(email))
                {
                    return CreateError("ExternalAuthFailed");
                }

                if (root.TryGetProperty("givenName", out var givenNameProp)) firstName = givenNameProp.GetString() ?? "";
                if (root.TryGetProperty("surname", out var surnameProp)) lastName = surnameProp.GetString() ?? "";
            }
            else
            {
                return CreateError("ProviderNotSupported");
            }
        }
        catch (Exception)
        {
            return CreateError("ExternalAuthException");
        }

        if (string.IsNullOrEmpty(email))
        {
            return CreateError("EmailEmpty");
        }

        email = email.ToLower();

        // 1. Check if user exists
        var user = await userRepository.GetFirstByConditionAsync(x => x.Email == email, token: cancellationToken);
        if (user is null)
        {
            // Auto-provision user
            user = new User
            {
                UserId = new UserId(IdGenerator.NextGuid()),
                Email = email,
                UserName = email,
                NormalizedUserName = email.ToUpper(),
                FirstName = firstName,
                LastName = lastName,
                SearchHint = $"{firstName} {lastName}".Trim().GenerateSearchHint(),
                IsActivated = true,
                CreatedTime = DateTime.UtcNow,
                SecurityStamp = IdGenerator.NextGuid().ToString(),
                LockoutEnabled = false
            };
            user.Id = user.UserId.Value;

            await userRepository.CreateOneAsync(user, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // 2. Generate Auth Tokens
        var tokenGenResult = await sender.Send(new TokenGeneratorCommand(user), cancellationToken);
        if (tokenGenResult.IsT1)
        {
            return CreateError("TokenGenerationFailed");
        }

        var identitySuccess = tokenGenResult.AsT0;

        // 3. Save Refresh Token
        var refreshToken = new RefreshToken
        {
            Id = new RefreshTokenId(IdGenerator.NextGuid()),
            UserToken = identitySuccess.UserToken,
            TokenExpiryTime = identitySuccess.TokenExpiryTime ?? DateTime.UtcNow.AddMinutes(15),
            ExpiryDate = identitySuccess.RefreshTokenExpiryTime ?? DateTime.UtcNow.AddDays(7),
            UserId = user.UserId,
            JwtId = identitySuccess.JwtId,
            CreationDate = identitySuccess.CreationDate
        };

        await refreshTokenRepository.CreateOneAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthenticationSuccessResponse
        {
            Token = identitySuccess.UserToken,
            RefreshToken = refreshToken.Id.ToString(),
            ExpiredIn = identitySuccess.TokenExpiryTime ?? DateTime.UtcNow.AddMinutes(15)
        };
    }

    private static ErrorDetailResponse CreateError(string code) => new()
    {
        Code = code,
        Messages = [code]
    };
}
