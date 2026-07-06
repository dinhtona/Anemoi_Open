using System.Threading;
using System.Threading.Tasks;
using Anemoi.Centralize.Application.Abstractions;
using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Commands;

public sealed class CreateMockRouteCommandHandler(IMockRouteRepository routeRepository)
    : IRequestHandler<CreateMockRouteCommand, MockRouteDto>
{
    public async Task<MockRouteDto> Handle(CreateMockRouteCommand request, CancellationToken cancellationToken)
    {
        var route = new MockRouteDto
        {
            Method = request.Method.Trim().ToUpperInvariant(),
            Path = "/" + request.Path.Trim('/'),
            StatusCode = request.StatusCode,
            ContentType = request.ContentType,
            ResponseBody = request.ResponseBody,
            Description = request.Description,
            IsActive = request.IsActive
        };

        await routeRepository.AddAsync(route);
        return route;
    }
}
