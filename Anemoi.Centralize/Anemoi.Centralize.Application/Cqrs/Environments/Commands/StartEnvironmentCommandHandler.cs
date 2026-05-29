using System.Threading;
using System.Threading.Tasks;
using Anemoi.Centralize.Application.Abstractions;
using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Commands;

public sealed class StartEnvironmentCommandHandler(IDockerService dockerService)
    : IRequestHandler<StartEnvironmentCommand, bool>
{
    public async Task<bool> Handle(StartEnvironmentCommand request, CancellationToken cancellationToken)
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

        return await dockerService.StartContainerAsync(containerName, cancellationToken);
    }
}
