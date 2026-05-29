using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.Centralize.Application.Abstractions;
using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Queries;

public sealed class GetMockRoutesQueryHandler(IMockRouteRepository routeRepository)
    : IRequestHandler<GetMockRoutesQuery, List<MockRouteDto>>
{
    public async Task<List<MockRouteDto>> Handle(GetMockRoutesQuery request, CancellationToken cancellationToken)
    {
        return await routeRepository.GetAllAsync();
    }
}
