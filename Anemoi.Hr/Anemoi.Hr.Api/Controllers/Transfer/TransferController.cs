using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.TransferCommands.SubmitTransfer;
using Anemoi.Hr.Application.Cqrs.Queries.TransferQueries.GetTransfers;
using Anemoi.Hr.Application.Cqrs.Queries.TransferQueries.GetTransferById;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Hr.Api.Controllers.Transfer;

[ApiController]
[Route("api/hr/transfers")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class TransferController(ISender sender) : ControllerBase
{
    [HttpPost("submit")]
    [HasPermission(HrPermissions.EmployeeTransferCreate)]
    [ProducesResponseType(typeof(EmployeeTransferDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Submit(
        [FromBody] SubmitTransferCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(HrPermissions.EmployeeTransferView)]
    [ProducesResponseType(typeof(PaginationResponse<EmployeeTransferDto>), StatusCodes.Status200OK)]
    public async Task<PaginationResponse<EmployeeTransferDto>> GetAll(
        [FromQuery] GetTransfersQuery query,
        CancellationToken cancellationToken)
    {
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("{id}")]
    [HasPermission(HrPermissions.TransferView)]
    [ProducesResponseType(typeof(EmployeeTransferDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(
        [FromRoute] EmployeeTransferId id,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetTransferByIdQuery(id), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}
