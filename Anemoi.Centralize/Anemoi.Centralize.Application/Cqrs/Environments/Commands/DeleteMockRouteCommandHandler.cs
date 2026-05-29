using System.Threading;
using System.Threading.Tasks;
using Anemoi.Centralize.Application.Abstractions;
using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Commands;

public sealed class DeleteMockRouteCommandHandler(IMockRouteRepository routeRepository)
    : IRequestHandler<DeleteMockRouteCommand, bool>
{
    public async Task<bool> Handle(DeleteMockRouteCommand request, CancellationToken cancellationToken)
    {
        var existing = await routeRepository.GetByIdAsync(request.Id);
        if (existing == null)
        {
            return false;
        }

        await routeRepository.DeleteAsync(request.Id);
        return true;
    }
}
