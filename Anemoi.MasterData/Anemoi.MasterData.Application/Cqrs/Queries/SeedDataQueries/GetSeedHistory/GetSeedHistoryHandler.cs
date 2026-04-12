using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Contract.MasterData.Queries.SeedDataQueries.GetSeedHistory;
using Anemoi.Contract.MasterData.Responses;
using Anemoi.MasterData.Application.Mappings;
using Anemoi.MasterData.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.MasterData.Application.Cqrs.Queries.SeedDataQueries.GetSeedHistory;

public sealed class GetSeedHistoryHandler(
    ISqlRepository<SeedHistory> historyRepository,
    MasterDataMapper mapper)
    : IRequestHandler<GetSeedHistoryQuery, List<SeedHistoryResponse>>
{
    public async Task<List<SeedHistoryResponse>> Handle(GetSeedHistoryQuery request, CancellationToken cancellationToken)
    {
        return await mapper.ProjectToSeedHistoryResponse(
            historyRepository.GetQueryable()
                .Where(x => x.SeedFunctionId == request.SeedFunctionId)
                .OrderByDescending(x => x.RunAt))
            .ToListAsync(cancellationToken);
    }
}
