using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.ActivateAllowanceType;
using Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.CreateAllowanceType;
using Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.DeactivateAllowanceType;
using Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.UpdateAllowanceType;
using Anemoi.Hr.Application.Cqrs.Queries.CompensationQueries.GetAllowanceTypeById;
using Anemoi.Hr.Application.Cqrs.Queries.CompensationQueries.GetAllowanceTypes;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.Compensation;

[ApiController]
[Route("api/hr/allowance-types")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class AllowanceTypeController(ISender sender) : ControllerBase
{
    [HttpGet]
    [HasPermission(HrPermissions.AllowanceTypeView)]
    [ProducesResponseType(typeof(PaginationResponse<AllowanceTypeResponse>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<AllowanceTypeResponse>> GetAllowanceTypes(
        [FromQuery] GetAllowanceTypesQuery query, CancellationToken cancellationToken)
    {
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.AllowanceTypeView)]
    [ProducesResponseType(typeof(AllowanceTypeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllowanceTypeById(
        [FromRoute] AllowanceTypeId id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetAllowanceTypeByIdQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.AllowanceTypeManage)]
    [ProducesResponseType(typeof(AllowanceTypeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateAllowanceType(
        [FromBody] CreateAllowanceTypeCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPut("{id}")]
    [HasPermission(HrPermissions.AllowanceTypeManage)]
    [ProducesResponseType(typeof(AllowanceTypeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAllowanceType(
        [FromRoute] AllowanceTypeId id,
        [FromBody] UpdateAllowanceTypeCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { AllowanceTypeId = id }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}/activate")]
    [HasPermission(HrPermissions.AllowanceTypeManage)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ActivateAllowanceType(
        [FromRoute] AllowanceTypeId id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new ActivateAllowanceTypeCommand(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost("{id}/deactivate")]
    [HasPermission(HrPermissions.AllowanceTypeManage)]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivateAllowanceType(
        [FromRoute] AllowanceTypeId id, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new DeactivateAllowanceTypeCommand(id, true), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}
