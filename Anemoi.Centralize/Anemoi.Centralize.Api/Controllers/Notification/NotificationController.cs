using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Authorization;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Contract.Notification.Commands.NotificationCommands.CreateNotification;
using Anemoi.Contract.Notification.Commands.NotificationCommands.HideAllReadNotifications;
using Anemoi.Contract.Notification.Commands.NotificationCommands.HideNotification;
using Anemoi.Contract.Notification.Commands.NotificationCommands.MarkAllAsRead;
using Anemoi.Contract.Notification.Commands.NotificationCommands.MarkAsRead;
using Anemoi.Contract.Notification.Commands.NotificationPreferenceCommands.CreateOrUpdateNotificationPreference;
using Anemoi.Contract.Notification.Commands.NotificationSettingsCommands.UpdateNotificationSettings;
using Anemoi.Contract.Notification.ModelIds;
using Anemoi.Contract.Notification.Queries.NotificationPreferenceQueries.GetMyNotificationPreference;
using Anemoi.Contract.Notification.Queries.NotificationQueries.GetNotifications;
using Anemoi.Contract.Notification.Queries.NotificationQueries.GetUnreadNotificationCount;
using Anemoi.Contract.Notification.Queries.NotificationSettingsQueries.GetNotificationSettings;
using Anemoi.Contract.Notification.Responses;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Centralize.Api.Controllers.Notification;

[Route("api/notification/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class NotificationController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.NotificationView)]
    [ProducesResponseType(typeof(PaginationResponse<NotificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetNotifications([FromQuery] int page, [FromQuery] int size, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var query = new GetNotificationsQuery(userId)
        {
            PageIndex = page <= 0 ? 1 : page,
            PageSize = size <= 0 ? 10 : size
        };
        var res = await sender.Send(query, cancellationToken);
        return Ok(res);
    }

    [HttpGet]
    [HasPermission(Permissions.NotificationView)]
    [ProducesResponseType(typeof(CountingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var query = new GetUnreadNotificationCountQuery(userId);
        var res = await sender.Send(query, cancellationToken);
        return Ok(res);
    }

    [HttpGet]
    [HasPermission(Permissions.NotificationView)]
    [ProducesResponseType(typeof(CollectionResponse<NotificationSettingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSettings(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var query = new GetNotificationSettingsQuery(userId);
        var res = await sender.Send(query, cancellationToken);
        return Ok(res);
    }

    [HttpPost]
    [HasPermission(Permissions.NotificationManage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateSettings([FromBody] List<NotificationSettingResponse> settings, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var command = new UpdateNotificationSettingsCommand(userId, settings);
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPost("{id}")]
    [HasPermission(Permissions.NotificationManage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var command = new MarkNotificationAsReadCommand(new NotificationHistoryId(id), userId);
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPost]
    [HasPermission(Permissions.NotificationManage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var command = new MarkAllNotificationsAsReadCommand(userId);
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPost("{id}")]
    [HasPermission(Permissions.NotificationManage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Hide(Guid id, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var command = new HideNotificationCommand(new NotificationHistoryId(id), userId);
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPost]
    [HasPermission(Permissions.NotificationManage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> HideAllRead(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var command = new HideAllReadNotificationsCommand(userId);
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpGet]
    [HasPermission(Permissions.NotificationPreferenceManage)]
    [ProducesResponseType(typeof(NotificationPreferenceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPreferences(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var query = new GetMyNotificationPreferenceQuery(userId);
        var res = await sender.Send(query, cancellationToken);
        return Ok(res);
    }

    [HttpPut]
    [HasPermission(Permissions.NotificationPreferenceManage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdatePreferences(
        [FromBody] CreateOrUpdateNotificationPreferenceCommand command,
        CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var cmd = command with { UserId = userId };
        var res = await sender.Send(cmd, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }
}


