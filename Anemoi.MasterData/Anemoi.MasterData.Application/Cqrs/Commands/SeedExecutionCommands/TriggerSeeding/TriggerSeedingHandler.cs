using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Errors;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Contract.MasterData.Commands.SeedExecutionCommands.TriggerSeeding;
using Anemoi.Contract.MasterData.Errors;
using Anemoi.MasterData.Application.Abstractions;
using Anemoi.MasterData.Application.Mappings;
using Anemoi.MasterData.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OneOf;
using Serilog;

namespace Anemoi.MasterData.Application.Cqrs.Commands.SeedExecutionCommands.TriggerSeeding;

public sealed class TriggerSeedingHandler(
    ISqlRepository<SeedFunction> functionRepository,
    ISqlRepository<SeedServer> serverRepository,
    IDbDiscoveryService dbDiscoveryService,
    IDataGeneratorService dataGeneratorService,
    IDataIngestionService dataIngestionService,
    MasterDataMapper mapper,
    ILogger logger)
    : IRequestHandler<TriggerSeedingCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(TriggerSeedingCommand request, CancellationToken cancellationToken)
    {
        var function = await functionRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == request.SeedFunctionId, cancellationToken);
        if (function is null) return mapper.ToErrorDetailResponse(MasterDataErrorDetail.SeedFunctionError.NotFound());

        var server = await serverRepository.GetFirstByConditionAsync(x => x.Id == function.SeedServerId);
        if (server is null) return mapper.ToErrorDetailResponse(MasterDataErrorDetail.SeedServerError.NotFound());

        var tables = JsonConvert.DeserializeObject<List<string>>(function.TablesJson) ?? new();
        var templateConfig = function.SeedTemplate?.ConfigJson;

        foreach (var tableName in tables)
        {
            logger.Information("[TriggerSeeding] Processing table {TableName} for server {ServerName}", tableName, server.Name);

            // 1. Discover Schema
            var schema = await dbDiscoveryService.GetSchemaAsync(server.ConnectionString, cancellationToken);
            var tableSchema = schema.Tables.FirstOrDefault(t => t.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (tableSchema is null)
            {
                logger.Warning("[TriggerSeeding] Table {TableName} not found in database schema", tableName);
                continue;
            }

            // 2. Generate Data
            var generatedData = await dataGeneratorService.GenerateDataAsync(tableSchema, 10, templateConfig, cancellationToken); // Default 10 records

            // 3. Ingest Data
            await dataIngestionService.IngestDataAsync(server.ConnectionString, tableName, generatedData, cancellationToken);
        }

        return None.Value;
    }
}
