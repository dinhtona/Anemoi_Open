using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Errors;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Contract.MasterData.Commands.SeedExecutionCommands.ClearSeedRowLogs;
using Anemoi.Contract.MasterData.Errors;
using Anemoi.MasterData.Application.Mappings;
using Anemoi.MasterData.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.MasterData.Application.Cqrs.Commands.SeedExecutionCommands.ClearSeedRowLogs;

public sealed class ClearSeedRowLogsHandler(
    ISqlRepository<SeedRowLog> rowLogRepository,
    IUnitOfWork unitOfWork,
    MasterDataMapper mapper,
    ILogger logger)
    : IRequestHandler<ClearSeedRowLogsCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(ClearSeedRowLogsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var logs = await rowLogRepository.GetQueryable()
                .Where(x => x.SeedServerId == request.SeedServerId)
                .ToListAsync(cancellationToken);

            if (logs.Any())
            {
                await rowLogRepository.RemoveManyAsync(logs, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                logger.Information("[ClearSeedRowLogs] Successfully cleared {Count} row logs for server {ServerId}", logs.Count, request.SeedServerId);
            }

            return None.Value;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "[ClearSeedRowLogs] Failed to clear row logs for server {ServerId}", request.SeedServerId);
            return mapper.ToErrorDetailResponse(MasterDataErrorDetail.SeedExecutionError.CustomError($"Clear logs failed: {ex.Message}"));
        }
    }
}
