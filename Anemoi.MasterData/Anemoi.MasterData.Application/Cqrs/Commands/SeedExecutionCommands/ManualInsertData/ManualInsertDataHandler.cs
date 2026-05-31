using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Errors;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Contract.MasterData.Commands.SeedExecutionCommands.ManualInsertData;
using Anemoi.Contract.MasterData.Errors;
using Anemoi.Contract.MasterData.ModelIds;
using Anemoi.Contract.MasterData.Responses;
using Anemoi.MasterData.Application.Abstractions;
using Anemoi.MasterData.Application.Mappings;
using Anemoi.MasterData.Domain.Models;
using MediatR;
using Newtonsoft.Json;
using OneOf;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.MasterData.Application.Cqrs.Commands.SeedExecutionCommands.ManualInsertData;

public sealed class ManualInsertDataHandler(
    ISqlRepository<SeedServer> serverRepository,
    ISqlRepository<SeedRowLog> rowLogRepository,
    IUnitOfWork unitOfWork,
    IDataIngestionService dataIngestionService,
    IDbDiscoveryService dbDiscoveryService,
    MasterDataMapper mapper,
    ILogger logger)
    : IRequestHandler<ManualInsertDataCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(ManualInsertDataCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var server = await serverRepository.GetFirstByConditionAsync(x => x.Id == request.SeedServerId);
            if (server is null)
                return mapper.ToErrorDetailResponse(MasterDataErrorDetail.SeedServerError.NotFound());

            logger.Information("[ManualInsertData] Inserting row into table {TableName} on server {ServerName}", 
                request.TableName, server.Name);

            var rows = new List<Dictionary<string, object>> { request.Data };
            await dataIngestionService.IngestDataAsync(
                server.ConnectionString, 
                server.Provider, 
                request.TableName, 
                rows, 
                cancellationToken);

            // Fetch table schema to resolve primary key
            var tableSchema = await dbDiscoveryService.GetTableSchemaAsync(server.ConnectionString, server.Provider, request.TableName, cancellationToken);
            var pkCondition = GetPrimaryKeyCondition(tableSchema, request.Data);

            // Save row log
            var rowLog = new SeedRowLog
            {
                Id = new SeedRowLogId(IdGenerator.NextGuid()),
                SeedServerId = request.SeedServerId,
                TableName = request.TableName,
                RunAt = DateTime.UtcNow,
                RowDataJson = JsonConvert.SerializeObject(request.Data),
                PrimaryKeyCondition = pkCondition
            };

            await rowLogRepository.CreateOneAsync(rowLog, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.Information("[ManualInsertData] Successfully inserted row into {TableName}", request.TableName);
            return None.Value;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "[ManualInsertData] Failed to insert row into {TableName}", request.TableName);
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
}
