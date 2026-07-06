using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.MasterData.ModelIds;
using Anemoi.Contract.MasterData.Queries.SeedDataQueries.GetSampleData;
using Anemoi.Contract.MasterData.Responses;
using Anemoi.MasterData.Application.Abstractions;
using Anemoi.MasterData.Application.Models;
using Anemoi.MasterData.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OneOf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.MasterData.Application.Cqrs.Queries.SeedDataQueries.GetSampleData;

public sealed class GetSampleDataHandler(
    ISqlRepository<SeedFunction> functionRepository,
    ISqlRepository<SeedServer> serverRepository,
    IDbDiscoveryService dbDiscoveryService,
    IDataGeneratorService dataGeneratorService)
    : IRequestHandler<GetSampleDataQuery, OneOf<SampleDataResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SampleDataResponse, ErrorDetailResponse>> Handle(GetSampleDataQuery request, CancellationToken cancellationToken)
    {
        try
        {
            SeedServer server = null;
            TableSchema tableSchema = null;
            TableConfig previewConfig = null;
            var activeRelationships = new List<RelationshipConfig>();
            var seedingContext = new Dictionary<string, List<Dictionary<string, object>>>();

            if (request.SeedFunctionId != null)
            {
                var function = await functionRepository.GetQueryable()
                    .Include(x => x.SeedTemplate)
                    .FirstOrDefaultAsync(x => x.Id == request.SeedFunctionId, cancellationToken);

                if (function is null)
                    return CreateError("SFE_03");

                server = await serverRepository.GetFirstByConditionAsync(x => x.Id == function.SeedServerId);
                if (server is null)
                    return CreateError("SSE_03");

                var configJson = function.SeedTemplate?.ConfigJson;
                if (string.IsNullOrWhiteSpace(configJson))
                    return CreateError("SEE_02");

                SeedTemplateConfig config;
                try
                {
                    config = JsonConvert.DeserializeObject<SeedTemplateConfig>(configJson);
                }
                catch
                {
                    return CreateError("SEE_03");
                }

                var tableConfig = config?.Tables.FirstOrDefault(t => t.TableName.Equals(request.TableName, StringComparison.OrdinalIgnoreCase));
                if (tableConfig is null)
                    return CreateError("SEE_04");

                tableSchema = await dbDiscoveryService.GetTableSchemaAsync(server.ConnectionString, server.Provider, request.TableName, cancellationToken);
                if (tableSchema is null)
                    return CreateError("SEE_05");

                previewConfig = new TableConfig
                {
                    TableName = tableConfig.TableName,
                    Order = tableConfig.Order,
                    RowCount = 1,
                    ColumnRules = tableConfig.ColumnRules
                };

                activeRelationships = config.Relationships
                    .Where(r => r.ChildTable.Equals(request.TableName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            else if (request.SeedServerId != null)
            {
                server = await serverRepository.GetFirstByConditionAsync(x => x.Id == request.SeedServerId);
                if (server is null)
                    return CreateError("SSE_03");

                tableSchema = await dbDiscoveryService.GetTableSchemaAsync(server.ConnectionString, server.Provider, request.TableName, cancellationToken);
                if (tableSchema is null)
                    return CreateError("SEE_05");

                previewConfig = new TableConfig
                {
                    TableName = request.TableName,
                    RowCount = 1,
                    ColumnRules = new List<ColumnRule>()
                };
            }
            else
            {
                return CreateError("SEE_06");
            }

            var generatedData = await dataGeneratorService.GenerateDataAsync(
                tableSchema,
                previewConfig,
                activeRelationships,
                seedingContext,
                cancellationToken);

            if (generatedData == null || !generatedData.Any())
                return CreateError("SEE_07");

            return new SampleDataResponse(generatedData[0]);
        }
        catch (Exception)
        {
            return CreateError("SEE_07");
        }
    }

    private static ErrorDetailResponse CreateError(string code) => new()
    {
        Code = code,
        Messages = [code]
    };
}
