using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.ProbationCommands.StartProbation;
using Anemoi.Hr.Application.Cqrs.Commands.ProbationCommands.PassProbation;
using Anemoi.Hr.Application.Cqrs.Commands.ProbationCommands.FailProbation;
using Anemoi.Hr.Application.Cqrs.Commands.ProbationCommands.ExtendProbation;
using Anemoi.Hr.Application.Cqrs.Queries.ProbationQueries.GetProbations;
using Anemoi.Hr.Application.Cqrs.Queries.ProbationQueries.GetProbationById;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.Probation;

[ApiController]
[Route("api/hr/probations")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class ProbationController(ISender sender) : ControllerBase
{
    [HttpPost]
    [HasPermission(HrPermissions.ProbationManage)]
    [ProducesResponseType(typeof(ProbationRecordDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Start(
        [FromBody] StartProbationCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}/pass")]
    [HasPermission(HrPermissions.ProbationManage)]
    [ProducesResponseType(typeof(ProbationRecordDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Pass(
        [FromRoute] ProbationRecordId id,
        [FromBody] PassProbationCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { Id = id }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}/fail")]
    [HasPermission(HrPermissions.ProbationManage)]
    [ProducesResponseType(typeof(ProbationRecordDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Fail(
        [FromRoute] ProbationRecordId id,
        [FromBody] FailProbationCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { Id = id }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}/extend")]
    [HasPermission(HrPermissions.ProbationManage)]
    [ProducesResponseType(typeof(ProbationRecordDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Extend(
        [FromRoute] ProbationRecordId id,
        [FromBody] ExtendProbationCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { Id = id }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.ProbationView)]
    [ProducesResponseType(typeof(PaginationResponse<ProbationRecordDto>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<ProbationRecordDto>> GetAll(
        [FromQuery] GetProbationsQuery query,
        CancellationToken cancellationToken)
    {
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.ProbationView)]
    [ProducesResponseType(typeof(ProbationRecordDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(
        [FromRoute] ProbationRecordId id,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetProbationByIdQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}
