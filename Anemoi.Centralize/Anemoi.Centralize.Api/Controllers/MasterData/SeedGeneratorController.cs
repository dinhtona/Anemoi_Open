using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.MasterData.Commands.SeedExecutionCommands.TriggerSeeding;
using Anemoi.Contract.MasterData.Commands.SeedExecutionCommands.ManualInsertData;
using Anemoi.Contract.MasterData.Commands.SeedFunctionCommands.CreateSeedFunction;
using Anemoi.Contract.MasterData.Commands.SeedFunctionCommands.UpdateSeedFunction;
using Anemoi.Contract.MasterData.Commands.SeedServerCommands.CreateSeedServer;
using Anemoi.Contract.MasterData.Commands.SeedServerCommands.TestSeedServerConnection;
using Anemoi.Contract.MasterData.Commands.SeedServerCommands.UpdateSeedServer;
using Anemoi.Contract.MasterData.Commands.SeedTemplateCommands.CreateSeedTemplate;
using Anemoi.Contract.MasterData.Commands.SeedTemplateCommands.UpdateSeedTemplate;
using Anemoi.Contract.MasterData.ModelIds;
using Anemoi.Contract.MasterData.Queries.SeedDataQueries.GetDbSchema;
using Anemoi.Contract.MasterData.Queries.SeedDataQueries.GetSeedFunctions;
using Anemoi.Contract.MasterData.Queries.SeedDataQueries.GetSeedServers;
using Anemoi.Contract.MasterData.Queries.SeedDataQueries.GetSeedTemplates;
using Anemoi.Contract.MasterData.Responses;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Centralize.Api.Controllers.MasterData;

[Route("api/masterData/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public sealed class SeedGeneratorController(ISender sender) : ControllerBase
{
    // --- Seed Server ---

    [HttpGet]
    [ProducesResponseType(typeof(PaginationResponse<SeedServerResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSeedServers([FromQuery] GetSeedServersQuery query, CancellationToken cancellationToken)
    {
        var res = await sender.Send(query, cancellationToken);
        return Ok(res);
    }

    [HttpPost]
    [Authorize(Policy = "Internal", Roles = "Administrator")]
    public async Task<IActionResult> CreateSeedServer([FromBody] CreateSeedServerCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPatch("{id}")]
    [Authorize(Policy = "Internal", Roles = "Administrator")]
    public async Task<IActionResult> UpdateSeedServer([FromRoute] SeedServerId id, [FromBody] UpdateSeedServerCommand command, CancellationToken cancellationToken)
    {
        var request = command with { Id = id };
        var res = await sender.Send(request, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPost]
    [Authorize(Policy = "Internal", Roles = "Administrator")]
    public async Task<IActionResult> TestSeedServerConnection([FromBody] TestSeedServerConnectionCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    // --- Seed Function ---

    [HttpGet]
    [ProducesResponseType(typeof(PaginationResponse<SeedFunctionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSeedFunctions([FromQuery] GetSeedFunctionsQuery query, CancellationToken cancellationToken)
    {
        var res = await sender.Send(query, cancellationToken);
        return Ok(res);
    }

    [HttpPost]
    [Authorize(Policy = "Internal", Roles = "Administrator")]
    public async Task<IActionResult> CreateSeedFunction([FromBody] CreateSeedFunctionCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPatch("{id}")]
    [Authorize(Policy = "Internal", Roles = "Administrator")]
    public async Task<IActionResult> UpdateSeedFunction([FromRoute] SeedFunctionId id, [FromBody] UpdateSeedFunctionCommand command, CancellationToken cancellationToken)
    {
        var request = command with { Id = id };
        var res = await sender.Send(request, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    // --- Seed Template ---

    [HttpGet]
    [ProducesResponseType(typeof(PaginationResponse<SeedTemplateResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSeedTemplates([FromQuery] GetSeedTemplatesQuery query, CancellationToken cancellationToken)
    {
        var res = await sender.Send(query, cancellationToken);
        return Ok(res);
    }

    [HttpPost]
    [Authorize(Policy = "Internal", Roles = "Administrator")]
    public async Task<IActionResult> CreateSeedTemplate([FromBody] CreateSeedTemplateCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPatch("{id}")]
    [Authorize(Policy = "Internal", Roles = "Administrator")]
    public async Task<IActionResult> UpdateSeedTemplate([FromRoute] SeedTemplateId id, [FromBody] UpdateSeedTemplateCommand command, CancellationToken cancellationToken)
    {
        var request = command with { Id = id };
        var res = await sender.Send(request, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    // --- Schema & Execution ---

    [HttpGet("{serverId}")]
    [ProducesResponseType(typeof(DbSchemaResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDbSchema([FromRoute] SeedServerId serverId, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetDbSchemaQuery(serverId), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [Authorize(Policy = "Internal", Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> TriggerSeeding([FromBody] TriggerSeedingCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPost]
    [Authorize(Policy = "Internal", Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ManualInsertData([FromBody] ManualInsertDataCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }
}
