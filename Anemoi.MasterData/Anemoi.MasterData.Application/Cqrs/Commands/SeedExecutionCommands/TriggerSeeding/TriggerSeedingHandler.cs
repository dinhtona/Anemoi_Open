using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Errors;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Contract.MasterData.Commands.SeedExecutionCommands.TriggerSeeding;
using Anemoi.Contract.MasterData.Errors;
using Anemoi.Contract.MasterData.ModelIds;
using Anemoi.Contract.MasterData.Responses;
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
    ISqlRepository<SeedHistory> historyRepository,
    MasterDataMapper mapper,
    ILogger logger)
    : IRequestHandler<TriggerSeedingCommand, OneOf<TriggerSeedingResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<TriggerSeedingResponse, ErrorDetailResponse>> Handle(TriggerSeedingCommand request, CancellationToken cancellationToken)
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

        try
        {
            // Parse Selected Tables from Function
            var selectedTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(function.TablesJson))
            {
                try {
                    var tables = JsonConvert.DeserializeObject<List<string>>(function.TablesJson);
                    if (tables != null)
                    {
                        foreach (var t in tables) selectedTables.Add(t);
                    }
                } catch {
                    logger.Warning("[TriggerSeeding] Failed to parse TablesJson for function {FunctionId}. Defaulting to all template tables.", function.Id);
                }
            }

            // Sort tables by defined Order
            // If no specific tables selected, include ALL tables from the template
            var sortedTables = config.Tables
                .Where(t => selectedTables.Count == 0 || selectedTables.Contains(t.TableName))
                .OrderBy(t => t.Order)
                .ToList();

            if (!sortedTables.Any())
                return mapper.ToErrorDetailResponse(MasterDataErrorDetail.SeedExecutionError.CustomError("None of the selected tables are defined in the template or no tables are configured."));

            logger.Information("[TriggerSeeding] Starting seeding for function {FunctionName} (ID: {FunctionId})", function.Name, function.Id);
            logger.Information("[TriggerSeeding] Template Config: {Config}", configJson);

            // Discover Full DB Schema once
            var dbSchema = await dbDiscoveryService.GetSchemaAsync(server.ConnectionString, server.Provider, cancellationToken);
            logger.Information("[TriggerSeeding] Discovered {TableCount} tables in target database", dbSchema.Tables.Count);

            // Track generated rows for Parent-Child mapping and Formulas
            var seedingContext = new Dictionary<string, List<Dictionary<string, object>>>();

            foreach (var tableConfig in sortedTables)
            {
                logger.Information("[TriggerSeeding] Processing table {TableName} (Order: {Order}, RowCount: {RowCount})", 
                    tableConfig.TableName, tableConfig.Order, tableConfig.RowCount);

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
                logger.Information("[TriggerSeeding] Generating data for {TableName}...", tableConfig.TableName);
                var generatedData = await dataGeneratorService.GenerateDataAsync(
                    tableSchema,
                    tableConfig,
                    activeRelationships,
                    seedingContext,
                    cancellationToken);

                if (generatedData != null && generatedData.Any())
                {
                    logger.Information("[TriggerSeeding] Generated {RowCount} rows for {TableName}. Sample Row: {SampleData}", 
                        generatedData.Count, tableConfig.TableName, JsonConvert.SerializeObject(generatedData[0]));
                    
                    // 2. Ingest Data into Target DB
                    // Filter out system-managed columns like 'timestamp' or 'rowversion' before ingestion
                    var readOnlyColumns = tableSchema.Columns
                        .Where(c => c.DataType.Contains("timestamp", StringComparison.OrdinalIgnoreCase) || 
                                   c.DataType.Contains("rowversion", StringComparison.OrdinalIgnoreCase))
                        .Select(c => c.ColumnName)
                        .ToList();

                    if (readOnlyColumns.Any())
                    {
                        foreach (var row in generatedData)
                        {
                            foreach (var col in readOnlyColumns) row.Remove(col);
                        }
                    }

                    logger.Information("[TriggerSeeding] Ingesting {RowCount} rows into {TableName}...", generatedData.Count, tableConfig.TableName);
                    await dataIngestionService.IngestDataAsync(server.ConnectionString, server.Provider, tableConfig.TableName, generatedData, cancellationToken);

                    // 3. Cache for subsequent parent/formula references
                    seedingContext[tableConfig.TableName] = generatedData;
                    logger.Information("[TriggerSeeding] Successfully seeded {TableName}.", tableConfig.TableName);
                }
                else
                {
                    logger.Warning("[TriggerSeeding] No data generated for table {TableName}.", tableConfig.TableName);
                }
            }

            // 4. Post-Processing for CHILD_SUM_OF
            foreach (var tableConfig in sortedTables)
            {
                foreach (var rule in tableConfig.ColumnRules.Where(r => r.RuleType == "CHILD_SUM_OF"))
                {
                    if (string.IsNullOrWhiteSpace(rule.ColumnName)) continue;
                    var childTableName = rule.Parameters.ElementAtOrDefault(0);
                    var childColumnsToSum = rule.Parameters.Skip(1).Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim()).ToList();
                    
                    if (string.IsNullOrEmpty(childTableName) || !childColumnsToSum.Any()) continue;

                    var relationship = config.Relationships.FirstOrDefault(r => 
                        r.ParentTable.Equals(tableConfig.TableName, StringComparison.OrdinalIgnoreCase) && 
                        r.ChildTable.Equals(childTableName, StringComparison.OrdinalIgnoreCase));
                    
                    if (relationship == null || !relationship.JoinKeys.Any()) continue;
                    var joinKey = relationship.JoinKeys.First();

                    if (seedingContext.TryGetValue(childTableName, out var childRows) && 
                        seedingContext.TryGetValue(tableConfig.TableName, out var parentRows))
                    {
                        var updateData = new Dictionary<object, object>();

                        // Group child rows by ParentKey and sum the configured columns
                        var childSums = childRows
                            .Where(r => r.ContainsKey(joinKey.ChildColumn) && r[joinKey.ChildColumn] != null)
                            .GroupBy(r => r[joinKey.ChildColumn].ToString())
                            .ToDictionary(
                                g => g.Key, 
                                g => g.Sum(r => 
                                {
                                    decimal rowSum = 0;
                                    foreach(var col in childColumnsToSum)
                                    {
                                        if (r.TryGetValue(col, out var val) && decimal.TryParse(val?.ToString(), out var d))
                                        {
                                            rowSum += d;
                                        }
                                    }
                                    return rowSum;
                                })
                            );

                        // Map to Parent row and gather updates
                        foreach (var pRow in parentRows)
                        {
                            if (pRow.TryGetValue(joinKey.ParentColumn, out var parentKeyVal) && parentKeyVal != null)
                            {
                                var pKeyStr = parentKeyVal.ToString();
                                if (childSums.TryGetValue(pKeyStr, out var sumVal))
                                {
                                    updateData[parentKeyVal] = sumVal;
                                    pRow[rule.ColumnName] = sumVal;
                                }
                            }
                        }

                        if (updateData.Any())
                        {
                            logger.Information("[TriggerSeeding] Updating {RowCount} rows for CHILD_SUM_OF on {TableName}.{ColumnName}", updateData.Count, tableConfig.TableName, rule.ColumnName);
                            await dataIngestionService.UpdateColumnDataAsync(server.ConnectionString, server.Provider, tableConfig.TableName, joinKey.ParentColumn, rule.ColumnName, updateData, cancellationToken);
                        }
                    }
                }
            }

            // 5. Record History
            var history = new SeedHistory
            {
                Id = SeedHistoryId.New(),
                SeedFunctionId = function.Id,
                RunAt = DateTime.UtcNow,
                ConfigJson = configJson,
                ResultJson = JsonConvert.SerializeObject(new TriggerSeedingResponse(seedingContext))
            };
            await historyRepository.CreateOneAsync(history, cancellationToken);

            logger.Information("[TriggerSeeding] Seeding completed successfully for function {FunctionName}", function.Name);
            return new TriggerSeedingResponse(seedingContext);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "[TriggerSeeding] Unexpected error during seeding execution for function {FunctionId}", request.SeedFunctionId);
            return mapper.ToErrorDetailResponse(MasterDataErrorDetail.SeedExecutionError.CustomError($"Seeding failed: {ex.Message}"));
        }
    }
}
