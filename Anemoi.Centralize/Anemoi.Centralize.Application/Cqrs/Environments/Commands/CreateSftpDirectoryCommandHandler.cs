using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Anemoi.Centralize.Application.Abstractions;
using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Commands;

public sealed class CreateSftpDirectoryCommandHandler(
    ISftpFileManager fileManager,
    IEnvironmentNotificationService environmentNotificationService)
    : IRequestHandler<CreateSftpDirectoryCommand, bool>
{
    public async Task<bool> Handle(CreateSftpDirectoryCommand request, CancellationToken cancellationToken)
    {
        var result = fileManager.CreateDirectory(request.Path);
        if (result)
        {
            await environmentNotificationService.NotifyEnvironmentActivityAsync(
                "EnvironmentServiceSftp",
                "EnvironmentSftpDirectoryCreated",
                Path.GetFileName(request.Path),
                Path.GetDirectoryName(request.Path) ?? "/");
        }
        return result;
    }
}
