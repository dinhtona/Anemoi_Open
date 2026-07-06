using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Centralize.Application.Cqrs.Requests.Workspace;
using Anemoi.Contract.Identity.Commands.IdentityCommands.CheckEmailResetTokenCommand;
using Anemoi.Contract.Identity.Commands.IdentityCommands.ConfirmEmail;
using Anemoi.Contract.Identity.Commands.IdentityCommands.ResendRegistrationToken;
using Anemoi.Contract.Identity.Commands.IdentityCommands.UserChangePassword;
using Anemoi.Contract.Identity.Commands.IdentityCommands.UserForgetPassword;
using Anemoi.Contract.Identity.Commands.IdentityCommands.UserLogout;
using Anemoi.Contract.Identity.Commands.RefreshTokenCommands.GenerateTokenByRoleGroupClaims;
using Anemoi.Contract.Identity.Commands.RefreshTokenCommands.UserLogin;
using Anemoi.Contract.Identity.Commands.RefreshTokenCommands.UserRefreshToken;
using Anemoi.Contract.Identity.Commands.UserCommands.CreateUser;
using Anemoi.Contract.Identity.Contracts;
using Anemoi.Contract.Identity.Queries.IdentityQueries.CheckUserExist;
using Anemoi.Contract.Identity.Responses;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Anemoi.Contract.Identity.ModelIds;

namespace Anemoi.Centralize.Api.Controllers.Identity;

[Route("api/identity/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public class IdentityController(ISender sender, IWebHostEnvironment environment) : ControllerBase
{
    /// <summary>
    /// Check Account
    /// </summary>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserWithEmailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckAccount([FromQuery] CheckUserExistQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return result.Match<IActionResult>(Ok, BadRequest);
    }

    /// <summary>
    /// Active Account
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ActiveAccount([FromQuery] ConfirmEmailCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    /// <summary>
    /// Login
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("auth-limit")]
    [ProducesResponseType(typeof(AuthenticationSuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] UserLoginCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(
            success =>
            {
                SetTokenCookies(success.Token, success.RefreshToken);
                return Ok(success);
            },
            BadRequest);
    }

    /// <summary>
    /// GenerateWorkspaceToken
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(AuthenticationSuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateWorkspaceToken([FromBody] GenerateWorkspaceTokenRequest command,
        CancellationToken cancellationToken)
    {
        var res = await sender
            .Send(new GenerateTokenByRoleGroupClaimsCommand([
                new RoleGroupClaimContract("workspaceId", command.WorkspaceId)
            ]), cancellationToken);
        
        return res.Match<IActionResult>(
            success =>
            {
                SetTokenCookies(success.Token, success.RefreshToken);
                return Ok(success);
            },
            BadRequest);
    }

    /// <summary>
    /// RefreshToken
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("auth-limit")]
    [ProducesResponseType(typeof(AuthenticationSuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] UserRefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var refreshTokenStr = command.RefreshToken?.ToString();
        if (string.IsNullOrEmpty(refreshTokenStr))
        {
            Request.Cookies.TryGetValue("refresh_token", out refreshTokenStr);
        }

        if (string.IsNullOrEmpty(refreshTokenStr) || !Guid.TryParse(refreshTokenStr, out var guid))
        {
            return BadRequest(new ErrorDetailResponse { Code = "RefreshTokenInvalid", Messages = ["RefreshTokenInvalid"] });
        }

        var res = await sender.Send(new UserRefreshTokenCommand(new RefreshTokenId(guid)), cancellationToken);
        return res.Match<IActionResult>(
            success =>
            {
                SetTokenCookies(success.Token, success.RefreshToken);
                return Ok(success);
            },
            BadRequest);
    }


    /// <summary>
    /// Logout
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var token = HttpContext.GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            await sender.Send(new UserLogoutCommand(token), cancellationToken);
        }
        
        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");
        
        return Ok();
    }

    private void SetTokenCookies(string accessToken, string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Path = "/"
        };

        Response.Cookies.Append("access_token", accessToken, cookieOptions);
        Response.Cookies.Append("refresh_token", refreshToken, cookieOptions);
    }

    /// <summary>
    /// Register Account
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("auth-limit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RegisterAccount([FromBody] CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(request, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    /// <summary>
    /// ResendRegistrationToken
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("auth-limit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResendRegistrationToken([FromBody] ResendRegistrationTokenCommand request,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(request, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    /// <summary>
    /// Change Password
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] UserChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    /// <summary>
    /// Forget Password
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("auth-limit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ForgetPassword([FromBody] UserForgetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    /// <summary>
    /// Check Email Token
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CheckEmailResetToken(
        [FromBody] CheckEmailResetTokenCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    /// <summary>
    /// ExternalLogin
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("auth-limit")]
    [ProducesResponseType(typeof(AuthenticationSuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExternalLogin([FromBody] Anemoi.Contract.Identity.Commands.RefreshTokenCommands.ExternalLogin.ExternalLoginCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(
            success =>
            {
                SetTokenCookies(success.Token, success.RefreshToken);
                return Ok(success);
            },
            BadRequest);
    }
}
