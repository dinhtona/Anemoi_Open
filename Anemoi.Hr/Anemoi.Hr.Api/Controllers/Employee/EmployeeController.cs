using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.LinkEmployeesToIdentityUsers;
using Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.PromoteEmployee;
using Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.TransferEmployee;
using Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.GetEmployee;
using Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.GetEmployeeDepartmentHistory;
using Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.GetEmployeeGradeHistory;
using Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.GetEmployeePositionHistory;
using Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.GetEmployeePromotionTimeline;
using Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.GetEmployees;
using Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.GetMyEmployeeProfile;
using Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.SearchEmployees;
using Anemoi.Hr.Application.Cqrs.Queries.LifecycleQueries.GetEmployeeTimeline;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace Anemoi.Hr.Api.Controllers.Employee;

[ApiController]
[Route("api/hr/employee/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class EmployeeController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(HrPermissions.EmployeeView)]
    [ProducesResponseType(typeof(PaginationResponse<EmployeeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployees([FromQuery] GetEmployeesQuery query, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(query, cancellationToken));
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.EmployeeView)]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeById([FromRoute] EmployeeId id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetEmployeeQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.EmployeeView)]
    [ProducesResponseType(typeof(PaginationResponse<EmployeeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchEmployees([FromQuery] SearchEmployeesQuery query, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(query, cancellationToken));
    }

    [HttpGet]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var email = HttpContext.GetClaimValue("email");
        var res = await sender.Send(new GetMyEmployeeProfileQuery(userId, email), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.EmployeeIdentityLink)]
    [ProducesResponseType(typeof(EmployeeIdentityLinkResultResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> LinkEmployeesToIdentityUsers(
        [FromBody] LinkEmployeesToIdentityUsersCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CreatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.EmployeeTransferCreate)]
    [ProducesResponseType(typeof(EmployeeDepartmentHistoryIdResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> TransferEmployee(
        [FromBody] TransferEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CreatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.PromotionCreate)]
    [ProducesResponseType(typeof(PromoteEmployeeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> PromoteEmployee(
        [FromBody] PromoteEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CreatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("{employeeId}")]
    [HasPermission(HrPermissions.EmployeeTransferView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<EmployeeDepartmentHistoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeDepartmentHistory(
        [FromRoute] EmployeeId employeeId,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetEmployeeDepartmentHistoryQuery(employeeId), cancellationToken);
        return Ok(res);
    }

    [HttpGet("{employeeId}")]
    [HasPermission(HrPermissions.PositionChangeView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<EmployeePositionHistoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeePositionHistory(
        [FromRoute] EmployeeId employeeId,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetEmployeePositionHistoryQuery(employeeId), cancellationToken);
        return Ok(res);
    }

    [HttpGet("{employeeId}")]
    [HasPermission(HrPermissions.GradeChangeView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<EmployeeGradeHistoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeGradeHistory(
        [FromRoute] EmployeeId employeeId,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetEmployeeGradeHistoryQuery(employeeId), cancellationToken);
        return Ok(res);
    }

    [HttpGet("{employeeId}")]
    [HasPermission(HrPermissions.PromotionView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<EmployeePromotionTimelineResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeePromotionTimeline(
        [FromRoute] EmployeeId employeeId,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetEmployeePromotionTimelineQuery(employeeId), cancellationToken);
        return Ok(res);
    }

    [HttpGet("{id}/timeline")]
    [HasPermission(HrPermissions.EmployeeTimelineView)]
    [ProducesResponseType(typeof(PaginationResponse<EmployeeHistoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeTimeline(
        [FromRoute] EmployeeId id,
        [FromQuery] string? eventType,
        [FromQuery] string? entityType,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        CancellationToken cancellationToken)
    {
        var query = new GetEmployeeTimelineQuery(id, eventType, entityType, dateFrom, dateTo);
        return Ok(await sender.Send(query, cancellationToken));
    }

    [HttpGet("me/timeline")]
    [HasPermission(HrPermissions.EmployeeTimelineView)]
    [ProducesResponseType(typeof(PaginationResponse<EmployeeHistoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyTimeline(
        [FromQuery] string? eventType,
        [FromQuery] string? entityType,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var email = HttpContext.GetClaimValue("email");
        var profileRes = await sender.Send(
            new GetMyEmployeeProfileQuery(userId, email), cancellationToken);
        if (!profileRes.TryPickT0(out var profile, out var error))
            return BadRequest(error);

        var employeeId = new EmployeeId(Guid.Parse(profile.Id));
        var query = new GetEmployeeTimelineQuery(employeeId, eventType, entityType, dateFrom, dateTo);
        return Ok(await sender.Send(query, cancellationToken));
    }
}
