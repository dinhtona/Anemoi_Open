using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.CalendarException.CreateCalendarException;
using Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.CalendarException.DeleteCalendarException;
using Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.CalendarException.UpdateCalendarException;
using Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.CompanyHoliday.CreateCompanyHoliday;
using Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.CompanyHoliday.DeleteCompanyHoliday;
using Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.CompanyHoliday.UpdateCompanyHoliday;
using Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.PublicHoliday.CreatePublicHoliday;
using Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.PublicHoliday.DeletePublicHoliday;
using Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.PublicHoliday.UpdatePublicHoliday;
using Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.WorkingCalendarRule.ActivateWorkingCalendarRule;
using Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.WorkingCalendarRule.CreateWorkingCalendarRule;
using Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.WorkingCalendarRule.DeactivateWorkingCalendarRule;
using Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.WorkingCalendarRule.DeleteWorkingCalendarRule;
using Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.WorkingCalendarRule.UpdateWorkingCalendarRule;
using Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.CalendarException.GetCalendarExceptionById;
using Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.CalendarException.GetCalendarExceptions;
using Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.CalendarStatus.GetCalendarStatus;
using Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.CompanyHoliday.GetCompanyHolidayById;
using Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.CompanyHoliday.GetCompanyHolidays;
using Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.MonthlyCalendar.GetMonthlyCalendar;
using Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.PublicHoliday.GetPublicHolidayById;
using Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.PublicHoliday.GetPublicHolidays;
using Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.WorkingCalendarRule.GetWorkingCalendarRuleById;
using Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.WorkingCalendarRule.GetWorkingCalendarRules;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.CalendarManagement;

[ApiController]
[Route("api/hr/calendar-management/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class CalendarManagementController(ISender sender) : ControllerBase
{
    // === Public Holidays ===

    [HttpGet]
    [HasPermission(HrPermissions.CalendarView)]
    [ProducesResponseType(typeof(PaginationResponse<PublicHolidayResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublicHolidays([FromQuery] GetPublicHolidaysQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(query, cancellationToken));
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.CalendarView)]
    [ProducesResponseType(typeof(PublicHolidayResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublicHolidayById([FromRoute] PublicHolidayId id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPublicHolidayByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [HasPermission(HrPermissions.CalendarManage)]
    [ProducesResponseType(typeof(PublicHolidayIdResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreatePublicHoliday([FromBody] CreatePublicHolidayCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPut("{id}")]
    [HasPermission(HrPermissions.CalendarManage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePublicHoliday([FromRoute] PublicHolidayId id,
        [FromBody] UpdatePublicHolidayCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command with { Id = id }, cancellationToken);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpDelete("{id}")]
    [HasPermission(HrPermissions.CalendarManage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeletePublicHoliday([FromRoute] PublicHolidayId id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeletePublicHolidayCommand(id), cancellationToken);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    // === Company Holidays ===

    [HttpGet]
    [HasPermission(HrPermissions.CalendarView)]
    [ProducesResponseType(typeof(PaginationResponse<CompanyHolidayResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCompanyHolidays([FromQuery] GetCompanyHolidaysQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(query, cancellationToken));
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.CalendarView)]
    [ProducesResponseType(typeof(CompanyHolidayResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCompanyHolidayById([FromRoute] CompanyHolidayId id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCompanyHolidayByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [HasPermission(HrPermissions.CalendarManage)]
    [ProducesResponseType(typeof(CompanyHolidayIdResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateCompanyHoliday([FromBody] CreateCompanyHolidayCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPut("{id}")]
    [HasPermission(HrPermissions.CalendarManage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateCompanyHoliday([FromRoute] CompanyHolidayId id,
        [FromBody] UpdateCompanyHolidayCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command with { Id = id }, cancellationToken);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpDelete("{id}")]
    [HasPermission(HrPermissions.CalendarManage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteCompanyHoliday([FromRoute] CompanyHolidayId id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteCompanyHolidayCommand(id), cancellationToken);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    // === Working Calendar Rules ===

    [HttpGet]
    [HasPermission(HrPermissions.CalendarView)]
    [ProducesResponseType(typeof(PaginationResponse<WorkingCalendarRuleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkingCalendarRules([FromQuery] GetWorkingCalendarRulesQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(query, cancellationToken));
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.CalendarView)]
    [ProducesResponseType(typeof(WorkingCalendarRuleResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkingCalendarRuleById([FromRoute] WorkingCalendarRuleId id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetWorkingCalendarRuleByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [HasPermission(HrPermissions.CalendarManage)]
    [ProducesResponseType(typeof(WorkingCalendarRuleIdResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateWorkingCalendarRule([FromBody] CreateWorkingCalendarRuleCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPut("{id}")]
    [HasPermission(HrPermissions.CalendarManage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateWorkingCalendarRule([FromRoute] WorkingCalendarRuleId id,
        [FromBody] UpdateWorkingCalendarRuleCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command with { Id = id }, cancellationToken);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPost("{id}")]
    [HasPermission(HrPermissions.CalendarManage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ActivateWorkingCalendarRule([FromRoute] WorkingCalendarRuleId id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ActivateWorkingCalendarRuleCommand(id), cancellationToken);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPost("{id}")]
    [HasPermission(HrPermissions.CalendarManage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivateWorkingCalendarRule([FromRoute] WorkingCalendarRuleId id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeactivateWorkingCalendarRuleCommand(id), cancellationToken);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpDelete("{id}")]
    [HasPermission(HrPermissions.CalendarManage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteWorkingCalendarRule([FromRoute] WorkingCalendarRuleId id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteWorkingCalendarRuleCommand(id), cancellationToken);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    // === Calendar Exceptions ===

    [HttpGet]
    [HasPermission(HrPermissions.CalendarView)]
    [ProducesResponseType(typeof(PaginationResponse<CalendarExceptionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCalendarExceptions([FromQuery] GetCalendarExceptionsQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(query, cancellationToken));
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.CalendarView)]
    [ProducesResponseType(typeof(CalendarExceptionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCalendarExceptionById([FromRoute] CalendarExceptionId id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCalendarExceptionByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [HasPermission(HrPermissions.CalendarManage)]
    [ProducesResponseType(typeof(CalendarExceptionIdResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateCalendarException([FromBody] CreateCalendarExceptionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPut("{id}")]
    [HasPermission(HrPermissions.CalendarManage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateCalendarException([FromRoute] CalendarExceptionId id,
        [FromBody] UpdateCalendarExceptionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command with { Id = id }, cancellationToken);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpDelete("{id}")]
    [HasPermission(HrPermissions.CalendarManage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteCalendarException([FromRoute] CalendarExceptionId id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteCalendarExceptionCommand(id), cancellationToken);
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    // === Calendar Engine ===

    [HttpGet]
    [HasPermission(HrPermissions.CalendarView)]
    [ProducesResponseType(typeof(CalendarStatusResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCalendarStatus([FromQuery] GetCalendarStatusQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(query, cancellationToken));
    }

    [HttpGet]
    [HasPermission(HrPermissions.CalendarView)]
    [ProducesResponseType(typeof(MonthlyCalendarResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthlyCalendar([FromQuery] GetMonthlyCalendarQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(query, cancellationToken));
    }
}
