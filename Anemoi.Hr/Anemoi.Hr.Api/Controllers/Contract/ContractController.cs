using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.ContractCommands.CreateContract;
using Anemoi.Hr.Application.Cqrs.Commands.ContractCommands.TerminateContract;
using Anemoi.Hr.Application.Cqrs.Commands.ContractCommands.UpdateContract;
using Anemoi.Hr.Application.Cqrs.Queries.ContractQueries.GetContractDetail;
using Anemoi.Hr.Application.Cqrs.Queries.ContractQueries.GetEmployeeContracts;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Api.Controllers.Contract;

[ApiController]
[Route("api/hr/contract/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class ContractController(ISender sender) : ControllerBase
{
    [HttpPost]
    [HasPermission(HrPermissions.ContractCreate)]
    [ProducesResponseType(typeof(EmployeeContractDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateContract(
        [FromBody] CreateContractCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { CreatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPut]
    [HasPermission(HrPermissions.ContractUpdate)]
    [ProducesResponseType(typeof(EmployeeContractDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateContract(
        [FromBody] UpdateContractCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { UpdatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(HrPermissions.ContractTerminate)]
    [ProducesResponseType(typeof(EmployeeContractDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> TerminateContract(
        [FromBody] TerminateContractCommand command,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(command with { UpdatedBy = HttpContext.GetUserId() }, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("{employeeId}")]
    [HasPermission(HrPermissions.ContractView)]
    [ProducesResponseType(typeof(IReadOnlyCollection<EmployeeContractResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeContracts(
        [FromRoute] EmployeeId employeeId,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetEmployeeContractsQuery(employeeId), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet("{contractId}")]
    [HasPermission(HrPermissions.ContractView)]
    [ProducesResponseType(typeof(EmployeeContractDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContractDetail(
        [FromRoute] EmployeeContractId contractId,
        CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetContractDetailQuery(contractId), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }
}
