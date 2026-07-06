using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.EmployeeNoteCommands.ArchiveNote;
using Anemoi.Hr.Application.Cqrs.Commands.EmployeeNoteCommands.CreateNote;
using Anemoi.Hr.Application.Cqrs.Queries.EmployeeNoteQueries.GetEmployeeNote;
using Anemoi.Hr.Application.Cqrs.Queries.EmployeeNoteQueries.GetEmployeeNotes;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.EmployeeNote;

[ApiController]
[Route("api/hr/employees/{employeeId}/notes")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class EmployeeNoteController(ISender mediator) : ControllerBase
{
    [HttpGet]
    [HasPermission(HrPermissions.EmployeeNoteView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<EmployeeNoteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotes(Guid employeeId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetEmployeeNotesQuery(new EmployeeId(employeeId)), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.EmployeeNoteView)]
    [ProducesResponseType(typeof(EmployeeNoteResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNote(Guid employeeId, Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetEmployeeNoteQuery(new EmployeeNoteId(id)), cancellationToken);
        return result.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.EmployeeNoteManage)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateNote(Guid employeeId, [FromBody] CreateEmployeeNoteCommand command, CancellationToken cancellationToken)
    {
        if (employeeId != command.EmployeeId.Value)
            return BadRequest("EmployeeId mismatch");
        var result = await mediator.Send(command with { CreatedByUserId = HttpContext.GetUserId() }, cancellationToken);
        return result.Match<IActionResult>(_ => Created(), BadRequest);
    }

    [HttpPost("{id}/archive")]
    [HasPermission(HrPermissions.EmployeeNoteManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ArchiveNote(Guid employeeId, Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ArchiveEmployeeNoteCommand(new EmployeeNoteId(id), ArchivedByUserId: HttpContext.GetUserId()), cancellationToken);
        return result.Match<IActionResult>(_ => NoContent(), BadRequest);
    }
}
