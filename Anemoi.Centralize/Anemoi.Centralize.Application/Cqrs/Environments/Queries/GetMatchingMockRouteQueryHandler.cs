#nullable enable
using System.Threading;
using System.Threading.Tasks;
using Anemoi.Centralize.Application.Abstractions;
using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Queries;

public sealed class GetMatchingMockRouteQueryHandler(
    IMockRouteRepository routeRepository,
    IEnvironmentNotificationService environmentNotificationService)
    : IRequestHandler<GetMatchingMockRouteQuery, MockRouteDto?>
{
    public async Task<MockRouteDto?> Handle(GetMatchingMockRouteQuery request, CancellationToken cancellationToken)
    {
        var route = await routeRepository.GetMatchingRouteAsync(request.Path, request.Method);
        if (route is null)
            return null;

        await environmentNotificationService.NotifyEnvironmentActivityAsync(
            "EnvironmentServiceMockApi",
            "EnvironmentMockApiMatched",
            request.Method,
            request.Path,
            route.StatusCode);
        return route;
    }
}
