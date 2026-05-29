using System.Threading;
using System.Threading.Tasks;
using Anemoi.Centralize.Application.Abstractions;
using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Commands;

public sealed class StopEnvironmentCommandHandler(IDockerService dockerService)
    : IRequestHandler<StopEnvironmentCommand, bool>
{
    public async Task<bool> Handle(StopEnvironmentCommand request, CancellationToken cancellationToken)
    {
        string containerName = request.Id switch
        {
            "mail" => "smtp4dev_server",
            "sftp" => "sftp_server",
            _ => null
        };

        if (containerName == null)
        {
            return false;
        }

        return await dockerService.StopContainerAsync(containerName, cancellationToken);
    }
}
