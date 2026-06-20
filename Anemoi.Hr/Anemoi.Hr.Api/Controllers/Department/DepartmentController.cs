using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.DepartmentCommands.ActivateDepartment;
using Anemoi.Hr.Application.Cqrs.Commands.DepartmentCommands.CreateDepartment;
using Anemoi.Hr.Application.Cqrs.Commands.DepartmentCommands.DeactivateDepartment;
using Anemoi.Hr.Application.Cqrs.Commands.DepartmentCommands.UpdateDepartment;
using Anemoi.Hr.Application.Cqrs.Queries.DepartmentQueries.GetDepartmentById;
using Anemoi.Hr.Application.Cqrs.Queries.DepartmentQueries.GetDepartments;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.Department;

[ApiController]
[Route("api/hr/departments")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class DepartmentController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(HrPermissions.DepartmentView)]
    [ProducesResponseType(typeof(PaginationResponse<DepartmentResponse>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<DepartmentResponse>> GetDepartments(
        [FromQuery] GetDepartmentsQuery query, CancellationToken cancellationToken)
    {
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.DepartmentView)]
    [ProducesResponseType(typeof(DepartmentResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartmentById(
        [FromRoute] DepartmentId id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetDepartmentByIdQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.DepartmentManage)]
    [ProducesResponseType(typeof(DepartmentResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateDepartment(
        [FromBody] CreateDepartmentCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPut("{id}")]
    [HasPermission(HrPermissions.DepartmentManage)]
    [ProducesResponseType(typeof(DepartmentResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateDepartment(
        [FromRoute] DepartmentId id,
        [FromBody] UpdateDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { Id = id }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}/activate")]
    [HasPermission(HrPermissions.DepartmentManage)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ActivateDepartment(
        [FromRoute] DepartmentId id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new ActivateDepartmentCommand(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}/deactivate")]
    [HasPermission(HrPermissions.DepartmentManage)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivateDepartment(
        [FromRoute] DepartmentId id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new DeactivateDepartmentCommand(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}
