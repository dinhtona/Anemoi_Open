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
    ISqlRepository<SeedRowLog> rowLogRepository,
    IUnitOfWork unitOfWork,
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
            return mapper.ToErrorDetailResponse(MasterDataErrorDetail.SeedExecutionError.CustomError());

        SeedTemplateConfig config;
        try {
            config = JsonConvert.DeserializeObject<SeedTemplateConfig>(configJson);
        } catch {
            return mapper.ToErrorDetailResponse(MasterDataErrorDetail.SeedExecutionError.CustomError());
        }

        if (config == null || config.Tables.Count == 0)
            return mapper.ToErrorDetailResponse(MasterDataErrorDetail.SeedExecutionError.CustomError());

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

            // Topologically sort tables based on parent-child relationships
            var filteredTables = config.Tables
                .Where(t => selectedTables.Count == 0 || selectedTables.Contains(t.TableName))
                .ToList();

            if (!filteredTables.Any())
                return mapper.ToErrorDetailResponse(MasterDataErrorDetail.SeedExecutionError.CustomError());

            var sortedTables = SortTablesTopologically(filteredTables, config.Relationships);

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

                    // Log generated rows to repository
                    foreach (var row in generatedData)
                    {
                        var pkCondition = GetPrimaryKeyCondition(tableSchema, row);
                        var rowLog = new SeedRowLog
                        {
                            Id = SeedRowLogId.New(),
                            SeedServerId = function.SeedServerId,
                            TableName = tableConfig.TableName,
                            RunAt = DateTime.UtcNow,
                            RowDataJson = JsonConvert.SerializeObject(row),
                            PrimaryKeyCondition = pkCondition
                        };
                        await rowLogRepository.CreateOneAsync(rowLog, cancellationToken);
                    }

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

            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.Information("[TriggerSeeding] Seeding completed successfully for function {FunctionName}", function.Name);
            return new TriggerSeedingResponse(seedingContext);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "[TriggerSeeding] Unexpected error during seeding execution for function {FunctionId}", request.SeedFunctionId);
            return mapper.ToErrorDetailResponse(MasterDataErrorDetail.SeedExecutionError.CustomError());
        }
    }

    private string GetPrimaryKeyCondition(TableSchema tableSchema, Dictionary<string, object> rowData)
    {
        if (tableSchema == null || rowData == null) return string.Empty;

        // 1. Identity check
        var identityCol = tableSchema.Columns.FirstOrDefault(c => c.IsIdentity);
        if (identityCol != null && rowData.TryGetValue(identityCol.ColumnName, out var identityVal) && identityVal != null)
        {
            return $"{identityCol.ColumnName}={FormatValue(identityVal)}";
        }

        // 2. PK name check (id, memid, memberid, etc.)
        var pkNames = new[] { "id", "memid", "memberid", $"{tableSchema.TableName.ToLower()}id", $"{tableSchema.TableName.ToLower()}_id" };
        foreach (var name in pkNames)
        {
            var col = tableSchema.Columns.FirstOrDefault(c => c.ColumnName.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (col != null && rowData.TryGetValue(col.ColumnName, out var pkVal) && pkVal != null)
            {
                return $"{col.ColumnName}={FormatValue(pkVal)}";
            }
        }

        // 3. Ending with id check
        var endsWithIdCol = tableSchema.Columns.FirstOrDefault(c => c.ColumnName.EndsWith("id", StringComparison.OrdinalIgnoreCase));
        if (endsWithIdCol != null && rowData.TryGetValue(endsWithIdCol.ColumnName, out var endsWithIdVal) && endsWithIdVal != null)
        {
            return $"{endsWithIdCol.ColumnName}={FormatValue(endsWithIdVal)}";
        }

        // 4. Default to first column
        if (tableSchema.Columns.Count > 0)
        {
            var col = tableSchema.Columns[0];
            if (rowData.TryGetValue(col.ColumnName, out var firstVal) && firstVal != null)
            {
                return $"{col.ColumnName}={FormatValue(firstVal)}";
            }
        }

        return string.Empty;
    }

    private string FormatValue(object val)
    {
        if (val is int || val is long || val is double || val is decimal || val is float)
            return val.ToString();
        return $"'{val.ToString().Replace("'", "''")}'";
    }

    private List<TableConfig> SortTablesTopologically(List<TableConfig> tables, List<RelationshipConfig> relationships)
    {
        var result = new List<TableConfig>();
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Build adjacency list: Parent -> List of Children
        var adj = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var table in tables)
        {
            adj[table.TableName] = new List<string>();
        }

        foreach (var rel in relationships)
        {
            if (adj.ContainsKey(rel.ParentTable) && adj.ContainsKey(rel.ChildTable))
            {
                adj[rel.ParentTable].Add(rel.ChildTable);
            }
        }

        void Dfs(string tableName)
        {
            if (visited.Contains(tableName)) return;
            if (visiting.Contains(tableName))
            {
                // Cycle detected - break to prevent infinite loops
                return;
            }

            visiting.Add(tableName);

            if (adj.TryGetValue(tableName, out var children))
            {
                foreach (var child in children)
                {
                    Dfs(child);
                }
            }

            visiting.Remove(tableName);
            visited.Add(tableName);

            var tableConfig = tables.FirstOrDefault(t => t.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (tableConfig != null)
            {
                result.Add(tableConfig);
            }
        }

        // Sort descending by Order first to preserve defined order for independent tables
        var orderedTables = tables.OrderByDescending(t => t.Order).ToList();
        foreach (var table in orderedTables)
        {
            Dfs(table.TableName);
        }

        result.Reverse();
        return result;
    }
}
