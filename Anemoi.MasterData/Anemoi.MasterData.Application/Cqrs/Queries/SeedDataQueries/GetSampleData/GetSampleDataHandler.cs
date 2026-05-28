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
                    return new ErrorDetailResponse { Messages = ["Seed function not found."], Code = "NotFound" };

                server = await serverRepository.GetFirstByConditionAsync(x => x.Id == function.SeedServerId);
                if (server is null)
                    return new ErrorDetailResponse { Messages = ["Seed server not found."], Code = "NotFound" };

                var configJson = function.SeedTemplate?.ConfigJson;
                if (string.IsNullOrWhiteSpace(configJson))
                    return new ErrorDetailResponse { Messages = ["Template configuration is missing."], Code = "BadRequest" };

                SeedTemplateConfig config;
                try
                {
                    config = JsonConvert.DeserializeObject<SeedTemplateConfig>(configJson);
                }
                catch
                {
                    return new ErrorDetailResponse { Messages = ["Failed to parse template configuration."], Code = "BadRequest" };
                }

                var tableConfig = config?.Tables.FirstOrDefault(t => t.TableName.Equals(request.TableName, StringComparison.OrdinalIgnoreCase));
                if (tableConfig is null)
                    return new ErrorDetailResponse { Messages = [$"Table {request.TableName} not found in template."], Code = "NotFound" };

                tableSchema = await dbDiscoveryService.GetTableSchemaAsync(server.ConnectionString, server.Provider, request.TableName, cancellationToken);
                if (tableSchema is null)
                    return new ErrorDetailResponse { Messages = [$"Table {request.TableName} not found in target database."], Code = "NotFound" };

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
                    return new ErrorDetailResponse { Messages = ["Seed server not found."], Code = "NotFound" };

                tableSchema = await dbDiscoveryService.GetTableSchemaAsync(server.ConnectionString, server.Provider, request.TableName, cancellationToken);
                if (tableSchema is null)
                    return new ErrorDetailResponse { Messages = [$"Table {request.TableName} not found in target database."], Code = "NotFound" };

                previewConfig = new TableConfig
                {
                    TableName = request.TableName,
                    RowCount = 1,
                    ColumnRules = new List<ColumnRule>()
                };
            }
            else
            {
                return new ErrorDetailResponse { Messages = ["Either SeedFunctionId or SeedServerId must be provided."], Code = "BadRequest" };
            }

            var generatedData = await dataGeneratorService.GenerateDataAsync(
                tableSchema,
                previewConfig,
                activeRelationships,
                seedingContext,
                cancellationToken);

            if (generatedData == null || !generatedData.Any())
                return new ErrorDetailResponse { Messages = ["Failed to generate sample data."], Code = "BadRequest" };

            return new SampleDataResponse(generatedData[0]);
        }
        catch (Exception ex)
        {
            return new ErrorDetailResponse { Messages = [ex.Message], Code = "InternalError" };
        }
    }
}
