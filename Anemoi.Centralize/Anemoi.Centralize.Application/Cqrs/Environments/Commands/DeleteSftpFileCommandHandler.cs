using System.Threading;
using System.Threading.Tasks;
using Anemoi.Centralize.Application.Abstractions;
using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Commands;

public sealed class DeleteSftpFileCommandHandler(
    ISftpFileManager fileManager,
    IEnvironmentNotificationService environmentNotificationService)
    : IRequestHandler<DeleteSftpFileCommand, bool>
{
    public async Task<bool> Handle(DeleteSftpFileCommand request, CancellationToken cancellationToken)
    {
        var result = fileManager.DeleteFile(request.Path);
        if (result)
        {
            await environmentNotificationService.NotifyEnvironmentActivityAsync(
                "EnvironmentServiceSftp",
                "EnvironmentSftpDeleted",
                request.Path);
        }
        return result;
    }
}
