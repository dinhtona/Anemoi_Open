using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.LinkEmployeesToIdentityUsers;
using Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.GetEmployee;
using Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.GetEmployees;
using Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.GetMyEmployeeProfile;
using Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.SearchEmployees;
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
}
