using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.ShiftManagementCommands.CreateShiftTemplate;
using Anemoi.Hr.Application.Cqrs.Commands.ShiftManagementCommands.UpdateShiftTemplate;
using Anemoi.Hr.Application.Cqrs.Commands.ShiftManagementCommands.ActivateShiftTemplate;
using Anemoi.Hr.Application.Cqrs.Commands.ShiftManagementCommands.DeactivateShiftTemplate;
using Anemoi.Hr.Application.Cqrs.Commands.ShiftManagementCommands.AssignShiftToEmployee;
using Anemoi.Hr.Application.Cqrs.Commands.ShiftManagementCommands.BulkAssignShift;
using Anemoi.Hr.Application.Cqrs.Commands.ShiftManagementCommands.CancelEmployeeShiftAssignment;
using Anemoi.Hr.Application.Cqrs.Queries.ShiftManagementQueries.GetShiftTemplateById;
using Anemoi.Hr.Application.Cqrs.Queries.ShiftManagementQueries.GetShiftTemplates;
using Anemoi.Hr.Application.Cqrs.Queries.ShiftManagementQueries.GetEmployeeShiftAssignmentById;
using Anemoi.Hr.Application.Cqrs.Queries.ShiftManagementQueries.GetEmployeeShiftAssignments;
using Anemoi.Hr.Application.Cqrs.Queries.ShiftManagementQueries.GetEmployeeScheduleCalendar;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.ShiftManagement;

[ApiController]
[Route("api/hr/shift-management/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class ShiftManagementController(ISender sender) : ControllerBase
{
    // === Shift Templates ===

    [HttpGet]
    [HasPermission(HrPermissions.ShiftView)]
    [ProducesResponseType(typeof(PaginationResponse<ShiftTemplateResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShiftTemplates([FromQuery] GetShiftTemplatesQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(query, cancellationToken));
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.ShiftView)]
    [ProducesResponseType(typeof(ShiftTemplateResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShiftTemplateById([FromRoute] ShiftTemplateId id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetShiftTemplateByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [HasPermission(HrPermissions.ShiftManage)]
    [ProducesResponseType(typeof(ShiftTemplateIdResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateShiftTemplate([FromBody] CreateShiftTemplateCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPut("{id}")]
    [HasPermission(HrPermissions.ShiftManage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateShiftTemplate([FromRoute] ShiftTemplateId id,
        [FromBody] UpdateShiftTemplateCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command with { Id = id }, cancellationToken);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPost("{id}")]
    [HasPermission(HrPermissions.ShiftManage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ActivateShiftTemplate([FromRoute] ShiftTemplateId id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ActivateShiftTemplateCommand(id), cancellationToken);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPost("{id}")]
    [HasPermission(HrPermissions.ShiftManage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivateShiftTemplate([FromRoute] ShiftTemplateId id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeactivateShiftTemplateCommand(id), cancellationToken);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    // === Shift Assignments ===

    [HttpGet]
    [HasPermission(HrPermissions.ShiftView)]
    [ProducesResponseType(typeof(PaginationResponse<EmployeeShiftAssignmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeShiftAssignments([FromQuery] GetEmployeeShiftAssignmentsQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(query, cancellationToken));
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.ShiftView)]
    [ProducesResponseType(typeof(EmployeeShiftAssignmentResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeShiftAssignmentById([FromRoute] EmployeeShiftAssignmentId id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetEmployeeShiftAssignmentByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [HasPermission(HrPermissions.ShiftAssign)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignShiftToEmployee([FromBody] AssignShiftToEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.ShiftAssign)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> BulkAssignShift([FromBody] BulkAssignShiftCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPost("{id}")]
    [HasPermission(HrPermissions.ShiftCancel)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelEmployeeShiftAssignment([FromRoute] EmployeeShiftAssignmentId id,
        [FromBody] CancelEmployeeShiftAssignmentCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command with { Id = id }, cancellationToken);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    // === Schedule Calendar ===

    [HttpGet]
    [HasPermission(HrPermissions.ShiftView)]
    [ProducesResponseType(typeof(List<EmployeeScheduleCalendarResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeScheduleCalendar([FromQuery] GetEmployeeScheduleCalendarQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(query, cancellationToken));
    }
}
