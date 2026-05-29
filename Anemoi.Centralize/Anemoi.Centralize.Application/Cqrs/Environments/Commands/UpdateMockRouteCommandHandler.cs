#nullable enable
using System.Threading;
using System.Threading.Tasks;
using Anemoi.Centralize.Application.Abstractions;
using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Commands;

public sealed class UpdateMockRouteCommandHandler(IMockRouteRepository routeRepository)
    : IRequestHandler<UpdateMockRouteCommand, MockRouteDto?>
{
    public async Task<MockRouteDto?> Handle(UpdateMockRouteCommand request, CancellationToken cancellationToken)
    {
        var existing = await routeRepository.GetByIdAsync(request.Id);
        if (existing == null)
        {
            return null;
        }

        existing.Method = request.Method.Trim().ToUpperInvariant();
        existing.Path = "/" + request.Path.Trim('/');
        existing.StatusCode = request.StatusCode;
        existing.ContentType = request.ContentType;
        existing.ResponseBody = request.ResponseBody;
        existing.Description = request.Description;
        existing.IsActive = request.IsActive;

        await routeRepository.UpdateAsync(existing);
        return existing;
    }
}
