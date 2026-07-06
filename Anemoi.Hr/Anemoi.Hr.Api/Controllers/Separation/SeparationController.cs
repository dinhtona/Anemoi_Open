using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.SeparationCommands.SubmitSeparation;
using Anemoi.Hr.Application.Cqrs.Queries.SeparationQueries.GetSeparations;
using Anemoi.Hr.Application.Cqrs.Queries.SeparationQueries.GetSeparationById;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.Separation;

[ApiController]
[Route("api/hr/separations")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class SeparationController(ISender sender) : ControllerBase
{
    [HttpPost("submit")]
    [HasPermission(HrPermissions.SeparationCreate)]
    [ProducesResponseType(typeof(EmployeeSeparationDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Submit(
        [FromBody] SubmitSeparationCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.SeparationView)]
    [ProducesResponseType(typeof(PaginationResponse<EmployeeSeparationDto>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<EmployeeSeparationDto>> GetAll(
        [FromQuery] GetSeparationsQuery query,
        CancellationToken cancellationToken)
    {
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.SeparationView)]
    [ProducesResponseType(typeof(EmployeeSeparationDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(
        [FromRoute] EmployeeSeparationId id,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetSeparationByIdQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}
