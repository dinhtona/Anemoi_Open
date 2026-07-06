using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.MasterData.Queries.SeedDataQueries.GetSeedRowLogs;
using Anemoi.Contract.MasterData.Responses;
using Anemoi.MasterData.Application.Mappings;
using Anemoi.MasterData.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.MasterData.Application.Cqrs.Queries.SeedDataQueries.GetSeedRowLogs;

public sealed class GetSeedRowLogsHandler(
    ISqlRepository<SeedRowLog> rowLogRepository,
    MasterDataMapper mapper)
    : IRequestHandler<GetSeedRowLogsQuery, OneOf<SeedRowLogsResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SeedRowLogsResponse, ErrorDetailResponse>> Handle(GetSeedRowLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = await rowLogRepository.GetQueryable()
            .Where(x => x.SeedServerId == request.SeedServerId)
            .OrderByDescending(x => x.RunAt)
            .ToListAsync(cancellationToken);

        var mappedLogs = logs.Select(mapper.ToSeedRowLogResponse).ToList();
        return new SeedRowLogsResponse(mappedLogs);
    }
}
