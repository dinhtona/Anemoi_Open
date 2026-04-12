using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Errors;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Contract.MasterData.Commands.SeedExecutionCommands.ManualInsertData;
using Anemoi.Contract.MasterData.Errors;
using Anemoi.MasterData.Application.Abstractions;
using Anemoi.MasterData.Application.Mappings;
using Anemoi.MasterData.Domain.Models;
using MediatR;
using OneOf;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.MasterData.Application.Cqrs.Commands.SeedExecutionCommands.ManualInsertData;

public sealed class ManualInsertDataHandler(
    ISqlRepository<SeedServer> serverRepository,
    IDataIngestionService dataIngestionService,
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

            logger.Information("[ManualInsertData] Successfully inserted row into {TableName}", request.TableName);
            return None.Value;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "[ManualInsertData] Failed to insert row into {TableName}", request.TableName);
            return mapper.ToErrorDetailResponse(MasterDataErrorDetail.SeedExecutionError.CustomError($"Manual insertion failed: {ex.Message}"));
        }
    }
}
