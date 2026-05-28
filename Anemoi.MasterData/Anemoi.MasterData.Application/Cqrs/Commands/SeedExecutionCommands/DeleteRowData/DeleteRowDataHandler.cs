using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Errors;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Contract.MasterData.Commands.SeedExecutionCommands.DeleteRowData;
using Anemoi.Contract.MasterData.Errors;
using Anemoi.MasterData.Application.Abstractions;
using Anemoi.MasterData.Application.Mappings;
using Anemoi.MasterData.Domain.Models;
using MediatR;
using OneOf;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.MasterData.Application.Cqrs.Commands.SeedExecutionCommands.DeleteRowData;

public sealed class DeleteRowDataHandler(
    ISqlRepository<SeedServer> serverRepository,
    ISqlRepository<SeedRowLog> rowLogRepository,
    IUnitOfWork unitOfWork,
    IDataIngestionService dataIngestionService,
    MasterDataMapper mapper,
    ILogger logger)
    : IRequestHandler<DeleteRowDataCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(DeleteRowDataCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var server = await serverRepository.GetFirstByConditionAsync(x => x.Id == request.SeedServerId);
            if (server is null)
                return mapper.ToErrorDetailResponse(MasterDataErrorDetail.SeedServerError.NotFound());

            logger.Information("[DeleteRowData] Deleting row from table {TableName} on server {ServerName} with condition {Condition}", 
                request.TableName, server.Name, request.PrimaryKeyCondition);

            await dataIngestionService.DeleteRowDataAsync(
                server.ConnectionString, 
                server.Provider, 
                request.TableName, 
                request.PrimaryKeyCondition, 
                cancellationToken);

            // Delete corresponding row log
            var log = await rowLogRepository.GetFirstByConditionAsync(x => 
                x.SeedServerId == request.SeedServerId && 
                x.TableName == request.TableName && 
                x.PrimaryKeyCondition == request.PrimaryKeyCondition);
            if (log != null)
            {
                await rowLogRepository.RemoveOneAsync(log, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            logger.Information("[DeleteRowData] Successfully deleted row from {TableName}", request.TableName);
            return None.Value;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "[DeleteRowData] Failed to delete row from {TableName}", request.TableName);
            return mapper.ToErrorDetailResponse(MasterDataErrorDetail.SeedExecutionError.CustomError($"Delete failed: {ex.Message}"));
        }
    }
}
