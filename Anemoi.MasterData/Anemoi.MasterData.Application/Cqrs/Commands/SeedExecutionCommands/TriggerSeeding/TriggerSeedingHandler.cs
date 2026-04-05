using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Errors;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Contract.MasterData.Commands.SeedExecutionCommands.TriggerSeeding;
using Anemoi.Contract.MasterData.Errors;
using Anemoi.MasterData.Application.Abstractions;
using Anemoi.MasterData.Application.Mappings;
using Anemoi.MasterData.Application.Models;
using Anemoi.MasterData.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OneOf;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            .Include(x => x.SeedTemplate)
            .FirstOrDefaultAsync(x => x.Id == request.SeedFunctionId, cancellationToken);
        if (function is null) 
            return mapper.ToErrorDetailResponse(MasterDataErrorDetail.SeedFunctionError.NotFound());

        var server = await serverRepository.GetFirstByConditionAsync(x => x.Id == function.SeedServerId);
        if (server is null) 
            return mapper.ToErrorDetailResponse(MasterDataErrorDetail.SeedServerError.NotFound());

        // Parse Template Config
        var configJson = function.SeedTemplate?.ConfigJson;
        if (string.IsNullOrWhiteSpace(configJson))
            return mapper.ToErrorDetailResponse(MasterDataErrorDetail.SeedExecutionError.CustomError("Template configuration is missing."));

        SeedTemplateConfig config;
        try {
            config = JsonConvert.DeserializeObject<SeedTemplateConfig>(configJson);
        } catch {
            return mapper.ToErrorDetailResponse(MasterDataErrorDetail.SeedExecutionError.CustomError("Failed to parse template configuration."));
        }

        if (config == null || config.Tables.Count == 0)
            return mapper.ToErrorDetailResponse(MasterDataErrorDetail.SeedExecutionError.CustomError("No tables configured in template."));

        // Discover Full DB Schema once
        var dbSchema = await dbDiscoveryService.GetSchemaAsync(server.ConnectionString, server.Provider, cancellationToken);

        // Track generated rows for Parent-Child mapping and Formulas
        var seedingContext = new Dictionary<string, List<Dictionary<string, object>>>();

        // Sort tables by defined Order
        var sortedTables = config.Tables.OrderBy(t => t.Order).ToList();

        foreach (var tableConfig in sortedTables)
        {
            logger.Information("[TriggerSeeding] Generating data for table {TableName} (Order: {Order})", tableConfig.TableName, tableConfig.Order);

            var tableSchema = dbSchema.Tables.FirstOrDefault(t => t.TableName.Equals(tableConfig.TableName, StringComparison.OrdinalIgnoreCase));
            if (tableSchema is null)
            {
                logger.Warning("[TriggerSeeding] Table {TableName} not found in DB Schema. Skipping.", tableConfig.TableName);
                continue;
            }

            // Identify relationships where this table is a child
            var activeRelationships = config.Relationships
                .Where(r => r.ChildTable.Equals(tableConfig.TableName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // 1. Generate Data using Seeding Context
            var generatedData = await dataGeneratorService.GenerateDataAsync(
                tableSchema, 
                tableConfig, 
                activeRelationships, 
                seedingContext, 
                cancellationToken);

            if (generatedData != null && generatedData.Count > 0)
            {
                // 2. Ingest Data into Target DB
                await dataIngestionService.IngestDataAsync(server.ConnectionString, server.Provider, tableConfig.TableName, generatedData, cancellationToken);
                
                // 3. Cache for subsequent parent/formula references
                seedingContext[tableConfig.TableName] = generatedData;
            }
        }

        return None.Value;
    }
}
