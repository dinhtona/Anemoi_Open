using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Authorization;
using Anemoi.BuildingBlock.Infrastructure.Authorization;
using Anemoi.Contract.MasterData.Commands.SeedExecutionCommands.TriggerSeeding;
using Anemoi.Contract.MasterData.Commands.SeedExecutionCommands.ManualInsertData;
using Anemoi.Contract.MasterData.Commands.SeedExecutionCommands.DeleteRowData;
using Anemoi.Contract.MasterData.Commands.SeedExecutionCommands.ClearSeedRowLogs;
using Anemoi.Contract.MasterData.Queries.SeedDataQueries.GetSeedRowLogs;
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
using Anemoi.Contract.MasterData.Queries.SeedDataQueries.GetSampleData;
using Anemoi.Contract.MasterData.Responses;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anemoi.Centralize.Api.Controllers.MasterData;

[Route("api/masterData/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = AuthorizationPolicies.Internal)]
[Produces("application/json")]
public sealed class SeedGeneratorController(ISender sender) : ControllerBase
{
    // --- Seed Server ---

    [HttpGet]
    [HasPermission(Permissions.SeedGeneratorRead)]
    [ProducesResponseType(typeof(PaginationResponse<SeedServerResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSeedServers([FromQuery] GetSeedServersQuery query, CancellationToken cancellationToken)
    {
        var res = await sender.Send(query, cancellationToken);
        return Ok(res);
    }

    [HttpPost]
    [HasPermission(Permissions.SeedGeneratorManage)]
    public async Task<IActionResult> CreateSeedServer([FromBody] CreateSeedServerCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPatch("{id}")]
    [HasPermission(Permissions.SeedGeneratorManage)]
    public async Task<IActionResult> UpdateSeedServer([FromRoute] SeedServerId id, [FromBody] UpdateSeedServerCommand command, CancellationToken cancellationToken)
    {
        var request = command with { Id = id };
        var res = await sender.Send(request, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPost]
    [HasPermission(Permissions.SeedGeneratorManage)]
    public async Task<IActionResult> TestSeedServerConnection([FromBody] TestSeedServerConnectionCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    // --- Seed Function ---

    [HttpGet]
    [HasPermission(Permissions.SeedGeneratorRead)]
    [ProducesResponseType(typeof(PaginationResponse<SeedFunctionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSeedFunctions([FromQuery] GetSeedFunctionsQuery query, CancellationToken cancellationToken)
    {
        var res = await sender.Send(query, cancellationToken);
        return Ok(res);
    }

    [HttpPost]
    [HasPermission(Permissions.SeedGeneratorManage)]
    public async Task<IActionResult> CreateSeedFunction([FromBody] CreateSeedFunctionCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPatch("{id}")]
    [HasPermission(Permissions.SeedGeneratorManage)]
    public async Task<IActionResult> UpdateSeedFunction([FromRoute] SeedFunctionId id, [FromBody] UpdateSeedFunctionCommand command, CancellationToken cancellationToken)
    {
        var request = command with { Id = id };
        var res = await sender.Send(request, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    // --- Seed Template ---

    [HttpGet]
    [HasPermission(Permissions.SeedGeneratorRead)]
    [ProducesResponseType(typeof(PaginationResponse<SeedTemplateResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSeedTemplates([FromQuery] GetSeedTemplatesQuery query, CancellationToken cancellationToken)
    {
        var res = await sender.Send(query, cancellationToken);
        return Ok(res);
    }

    [HttpPost]
    [HasPermission(Permissions.SeedGeneratorManage)]
    public async Task<IActionResult> CreateSeedTemplate([FromBody] CreateSeedTemplateCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPatch("{id}")]
    [HasPermission(Permissions.SeedGeneratorManage)]
    public async Task<IActionResult> UpdateSeedTemplate([FromRoute] SeedTemplateId id, [FromBody] UpdateSeedTemplateCommand command, CancellationToken cancellationToken)
    {
        var request = command with { Id = id };
        var res = await sender.Send(request, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    // --- Schema & Execution ---

    [HttpGet("{serverId}")]
    [HasPermission(Permissions.SeedGeneratorRead)]
    [ProducesResponseType(typeof(DbSchemaResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDbSchema([FromRoute] SeedServerId serverId, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetDbSchemaQuery(serverId), cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpGet]
    [HasPermission(Permissions.SeedGeneratorRead)]
    [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSampleData([FromQuery] GetSampleDataQuery query, CancellationToken cancellationToken)
    {
        var res = await sender.Send(query, cancellationToken);
        return res.Match<IActionResult>(val => Ok(val.Data), BadRequest);
    }

    [HttpPost]
    [HasPermission(Permissions.SeedExecutionRun)]
    [ProducesResponseType(typeof(TriggerSeedingResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> TriggerSeeding([FromBody] TriggerSeedingCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(Ok, BadRequest);
    }

    [HttpPost]
    [HasPermission(Permissions.SeedExecutionManualWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ManualInsertData([FromBody] ManualInsertDataCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpPost]
    [HasPermission(Permissions.SeedExecutionDeleteRow)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteRowData([FromBody] DeleteRowDataCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    [HttpGet("{serverId}")]
    [HasPermission(Permissions.SeedGeneratorRead)]
    [ProducesResponseType(typeof(List<SeedRowLogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSeedRowLogs([FromRoute] SeedServerId serverId, CancellationToken cancellationToken)
    {
        var res = await sender.Send(new GetSeedRowLogsQuery(serverId), cancellationToken);
        return res.Match<IActionResult>(val => Ok(val.Logs), BadRequest);
    }

    [HttpPost]
    [HasPermission(Permissions.SeedExecutionLogClear)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearSeedRowLogs([FromBody] ClearSeedRowLogsCommand command, CancellationToken cancellationToken)
    {
        var res = await sender.Send(command, cancellationToken);
        return res.Match<IActionResult>(_ => Ok(), BadRequest);
    }
}
