using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.AssignEmployeeAllowance;
using Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.AssignPositionAllowance;
using Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.CreateAllowanceType;
using Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.CreateSalaryGrade;
using Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.CreateSalaryRange;
using Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.TerminateEmployeeAllowance;
using Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.ChangeSalary;
using Anemoi.Hr.Application.Cqrs.Queries.CompensationQueries.GetAllowanceTypes;
using Anemoi.Hr.Application.Cqrs.Queries.CompensationQueries.GetCompensationDashboard;
using Anemoi.Hr.Application.Cqrs.Queries.CompensationQueries.GetCompensationSnapshot;
using Anemoi.Hr.Application.Cqrs.Queries.CompensationQueries.GetCompensationTimeline;
using Anemoi.Hr.Application.Cqrs.Queries.CompensationQueries.GetEmployeeAllowances;
using Anemoi.Hr.Application.Cqrs.Queries.CompensationQueries.GetEmployeeSalaryHistory;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Api.Controllers.Compensation;

[ApiController]
[Route("api/hr/compensation/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class CompensationController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(HrPermissions.AllowanceTypeView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<AllowanceTypeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllowanceTypes(CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetAllowanceTypesQuery(), cancellationToken);
        return Ok(res);
    }
    [HttpPost]
    [HasPermission(HrPermissions.SalaryChange)]
    [ProducesResponseType(typeof(ChangeSalaryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangeSalary(
        [FromBody] ChangeSalaryCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CreatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.EmployeeAllowanceChange)]
    [ProducesResponseType(typeof(AssignEmployeeAllowanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignEmployeeAllowance(
        [FromBody] AssignEmployeeAllowanceCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CreatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.EmployeeAllowanceChange)]
    [ProducesResponseType(typeof(TerminateEmployeeAllowanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> TerminateEmployeeAllowance(
        [FromBody] TerminateEmployeeAllowanceCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { UpdatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.SalaryGradeManage)]
    [ProducesResponseType(typeof(AssignPositionAllowanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignPositionAllowance(
        [FromBody] AssignPositionAllowanceCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.AllowanceTypeManage)]
    [ProducesResponseType(typeof(CreateAllowanceTypeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateAllowanceType(
        [FromBody] CreateAllowanceTypeCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.SalaryGradeManage)]
    [ProducesResponseType(typeof(CreateSalaryGradeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateSalaryGrade(
        [FromBody] CreateSalaryGradeCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.SalaryGradeManage)]
    [ProducesResponseType(typeof(CreateSalaryRangeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateSalaryRange(
        [FromBody] CreateSalaryRangeCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("{employeeId}")]
    [HasPermission(HrPermissions.SalaryView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<EmployeeSalaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeSalaryHistory(
        [FromRoute] EmployeeId employeeId,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetEmployeeSalaryHistoryQuery(employeeId), cancellationToken);
        return Ok(res);
    }

    [HttpGet("{employeeId}")]
    [HasPermission(HrPermissions.EmployeeAllowanceView)]
    [ProducesResponseType(typeof(EmployeeAllowancesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeAllowances(
        [FromRoute] EmployeeId employeeId,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetEmployeeAllowancesQuery(employeeId), cancellationToken);
        return Ok(res);
    }

    [HttpGet("{employeeId}")]
    [HasPermission(HrPermissions.SalaryView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<CompensationTimelineItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCompensationTimeline(
        [FromRoute] EmployeeId employeeId,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetCompensationTimelineQuery(employeeId), cancellationToken);
        return Ok(res);
    }

    [HttpGet]
    [HasPermission(HrPermissions.SalaryView)]
    [ProducesResponseType(typeof(CompensationSnapshotResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCompensationSnapshot(
        [FromQuery] EmployeeId employeeId,
        [FromQuery] DateOnly referenceDate,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetCompensationSnapshotQuery(employeeId, referenceDate), cancellationToken);
        return Ok(res);
    }

    [HttpGet]
    [HasPermission(HrPermissions.CompensationDashboardView)]
    [ProducesResponseType(typeof(CompensationDashboardResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCompensationDashboard(CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetCompensationDashboardQuery(), cancellationToken);
        return Ok(res);
    }
}
