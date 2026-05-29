using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.Centralize.Application.Abstractions;
using Anemoi.Centralize.Application.Configurations;
using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Queries;

public sealed class GetEnvironmentsQueryHandler(IDockerService dockerService, DevEnvironmentsSetting settings)
    : IRequestHandler<GetEnvironmentsQuery, List<ContainerStatusDto>>
{
    public async Task<List<ContainerStatusDto>> Handle(GetEnvironmentsQuery request, CancellationToken cancellationToken)
    {
        var mail = await dockerService.GetContainerStatusAsync(
            settings.MailContainerName,
            settings.MailDisplayName,
            settings.MailWebUiUrl,
            settings.MailDescription,
            cancellationToken
        );

        var sftp = await dockerService.GetContainerStatusAsync(
            settings.SftpContainerName,
            settings.SftpDisplayName,
            $"sftp://{settings.SftpUsername}:{settings.SftpPassword}@localhost:{settings.SftpPort}",
            settings.SftpDescription,
            cancellationToken
        );

        // API Test doesn't run in a separate container, it is always active in the centralize api
        var apiTest = new ContainerStatusDto(
            settings.MockApiContainerName,
            settings.MockApiDisplayName,
            "running",
            $"http://localhost:{settings.MockApiPort}{settings.MockApiBasePath}",
            settings.MockApiDescription,
            DateTime.UtcNow
        );

        return new List<ContainerStatusDto> { mail, sftp, apiTest };
    }
}
