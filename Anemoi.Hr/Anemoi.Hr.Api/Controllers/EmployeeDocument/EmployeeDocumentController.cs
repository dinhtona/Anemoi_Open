using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.EmployeeDocumentCommands.ArchiveDocument;
using Anemoi.Hr.Application.Cqrs.Commands.EmployeeDocumentCommands.CreateDocument;
using Anemoi.Hr.Application.Cqrs.Commands.EmployeeDocumentCommands.UpdateDocument;
using Anemoi.Hr.Application.Cqrs.Queries.DocumentQueries.GetEmployeeDocument;
using Anemoi.Hr.Application.Cqrs.Queries.DocumentQueries.GetEmployeeDocuments;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.EmployeeDocument;

[ApiController]
[Route("api/hr/employees/{employeeId}/documents")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class EmployeeDocumentController(ISender mediator) : ControllerBase
{
    [HttpGet]
    [HasPermission(HrPermissions.EmployeeDocumentView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<EmployeeDocumentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDocuments(Guid employeeId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetEmployeeDocumentsQuery(new EmployeeId(employeeId)), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.EmployeeDocumentView)]
    [ProducesResponseType(typeof(EmployeeDocumentResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDocument(Guid employeeId, Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetEmployeeDocumentQuery(new EmployeeDocumentId(id)), cancellationToken);
        return result.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.EmployeeDocumentManage)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateDocument(Guid employeeId, [FromBody] CreateEmployeeDocumentCommand command, CancellationToken cancellationToken)
    {
        if (employeeId != command.EmployeeId.Value)
            return BadRequest("EmployeeId mismatch");
        var result = await mediator.Send(command with { CreatedBy = HttpContext.GetUserId() }, cancellationToken);
        return result.Match<IActionResult>(_ => Created(), BadRequest);
    }

    [HttpPut("{id}")]
    [HasPermission(HrPermissions.EmployeeDocumentManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateDocument(Guid employeeId, Guid id, [FromBody] UpdateEmployeeDocumentCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id.Value)
            return BadRequest("Id mismatch");
        var result = await mediator.Send(command with { UpdatedBy = HttpContext.GetUserId() }, cancellationToken);
        return result.Match<IActionResult>(_ => NoContent(), BadRequest);
    }

    [HttpPost("{id}/archive")]
    [HasPermission(HrPermissions.EmployeeDocumentManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ArchiveDocument(Guid employeeId, Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ArchiveEmployeeDocumentCommand(new EmployeeDocumentId(id), ArchivedBy: HttpContext.GetUserId()), cancellationToken);
        return result.Match<IActionResult>(_ => NoContent(), BadRequest);
    }
}
