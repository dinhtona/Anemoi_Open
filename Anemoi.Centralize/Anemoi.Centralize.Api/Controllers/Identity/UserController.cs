using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Authorization;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Contract.Identity.Commands.UserCommands.UpdateUser;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Contract.Identity.Queries.UserQueries.GetUser;
using Anemoi.Contract.Identity.Queries.UserQueries.GetUsers;
using Anemoi.Contract.Identity.Responses;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Centralize.Api.Controllers.Identity;

[Route("api/identity/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public class UserController(ISender sender) : ControllerBase
{
    /// <summary>
    /// GetUsers
    /// </summary>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.Internal)]
    [HasPermission(Permissions.UserRead)]
    [ProducesResponseType(typeof(PaginationResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(query, cancellationToken);
        return Ok(res);
    }

    /// <summary>
    /// Get Profile
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var query = new GetUserQuery(new UserId(Guid.Parse(HttpContext.GetUserId())));
        var result = await sender.Send(query, cancellationToken);
        return result.Match<IActionResult>(Ok, BadRequest);
    }

    /// <summary>
    /// UpdateUser
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPatch]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    /// <summary>
    /// UpdateUserRoleGroups
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.Internal)]
    [HasPermission(Permissions.UserManage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateUserRoleGroups([FromBody] Anemoi.Contract.Identity.Commands.UserMapRoleGroupCommands.UpdateUserMapRoleGroups.UpdateUserMapRoleGroupsCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    /// <summary>
    /// RevokeUserSession
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.Internal)]
    [HasPermission(Permissions.UserManage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RevokeUserSession([FromBody] Anemoi.Contract.Identity.Commands.UserCommands.RevokeUserSession.RevokeUserSessionCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    /// <summary>
    /// PromoteSystemAdministrator
    /// </summary>
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.Internal, Roles = SystemRoles.Administrator)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PromoteSystemAdministrator(
        [FromBody] Anemoi.Contract.Identity.Commands.UserCommands.PromoteSystemAdministrator.PromoteSystemAdministratorCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    /// <summary>
    /// DemoteSystemAdministrator
    /// </summary>
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.Internal, Roles = SystemRoles.Administrator)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DemoteSystemAdministrator(
        [FromBody] Anemoi.Contract.Identity.Commands.UserCommands.DemoteSystemAdministrator.DemoteSystemAdministratorCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    /// <summary>
    /// GetOnlineUsers
    /// </summary>
    /// <param name="registry"></param>
    /// <returns></returns>
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.Internal)]
    [HasPermission(Permissions.UserRead)]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetOnlineUsers([FromServices] Anemoi.Centralize.Application.Abstractions.IConnectedUsersRegistry registry)
    {
        var activeUserIds = registry.GetActiveUserIds();
        return Ok(activeUserIds);
    }
}
