using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.EmployeeAssetCommands.ArchiveEmployeeAsset;
using Anemoi.Hr.Application.Cqrs.Commands.EmployeeAssetCommands.AssignEmployeeAsset;
using Anemoi.Hr.Application.Cqrs.Commands.EmployeeAssetCommands.MarkAssetDamaged;
using Anemoi.Hr.Application.Cqrs.Commands.EmployeeAssetCommands.MarkAssetLost;
using Anemoi.Hr.Application.Cqrs.Commands.EmployeeAssetCommands.ReturnEmployeeAsset;
using Anemoi.Hr.Application.Cqrs.Commands.EmployeeAssetCommands.UpdateEmployeeAsset;
using Anemoi.Hr.Application.Cqrs.Queries.AssetQueries.GetEmployeeAsset;
using Anemoi.Hr.Application.Cqrs.Queries.AssetQueries.GetEmployeeAssets;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.EmployeeAsset;

[ApiController]
[Route("api/hr/employees/{employeeId}/assets")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class EmployeeAssetController(ISender mediator) : ControllerBase
{
    [HttpGet]
    [HasPermission(HrPermissions.EmployeeAssetView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<EmployeeAssetResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssets(Guid employeeId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetEmployeeAssetsQuery(new EmployeeId(employeeId)), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.EmployeeAssetView)]
    [ProducesResponseType(typeof(EmployeeAssetResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAsset(Guid employeeId, Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetEmployeeAssetQuery(new EmployeeAssetId(id)), cancellationToken);
        return result.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.EmployeeAssetManage)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> AssignAsset(Guid employeeId, [FromBody] AssignEmployeeAssetCommand command, CancellationToken cancellationToken)
    {
        if (employeeId != command.EmployeeId.Value)
            return BadRequest("EmployeeId mismatch");
        var result = await mediator.Send(command with { CreatedBy = HttpContext.GetUserId() }, cancellationToken);
        return result.Match<IActionResult>(_ => Created(), BadRequest);
    }

    [HttpPut("{id}")]
    [HasPermission(HrPermissions.EmployeeAssetManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateAsset(Guid employeeId, Guid id, [FromBody] UpdateEmployeeAssetCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id.Value)
            return BadRequest("Id mismatch");
        var result = await mediator.Send(command with { UpdatedBy = HttpContext.GetUserId() }, cancellationToken);
        return result.Match<IActionResult>(_ => NoContent(), BadRequest);
    }

    [HttpPost("{id}/return")]
    [HasPermission(HrPermissions.EmployeeAssetManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReturnAsset(Guid employeeId, Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ReturnEmployeeAssetCommand(new EmployeeAssetId(id), ReturnedBy: HttpContext.GetUserId()), cancellationToken);
        return result.Match<IActionResult>(_ => NoContent(), BadRequest);
    }

    [HttpPost("{id}/lost")]
    [HasPermission(HrPermissions.EmployeeAssetManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAssetLost(Guid employeeId, Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new MarkAssetLostCommand(new EmployeeAssetId(id), UpdatedBy: HttpContext.GetUserId()), cancellationToken);
        return result.Match<IActionResult>(_ => NoContent(), BadRequest);
    }

    [HttpPost("{id}/damaged")]
    [HasPermission(HrPermissions.EmployeeAssetManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAssetDamaged(Guid employeeId, Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new MarkAssetDamagedCommand(new EmployeeAssetId(id), UpdatedBy: HttpContext.GetUserId()), cancellationToken);
        return result.Match<IActionResult>(_ => NoContent(), BadRequest);
    }

    [HttpPost("{id}/archive")]
    [HasPermission(HrPermissions.EmployeeAssetManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ArchiveAsset(Guid employeeId, Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ArchiveEmployeeAssetCommand(new EmployeeAssetId(id), ArchivedBy: HttpContext.GetUserId()), cancellationToken);
        return result.Match<IActionResult>(_ => NoContent(), BadRequest);
    }
}
