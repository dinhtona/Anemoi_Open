using System.Threading;
using System.Threading.Tasks;
using Anemoi.Centralize.Application.Abstractions;
using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Commands;

public sealed class UploadSftpFileCommandHandler(
    ISftpFileManager fileManager,
    IEnvironmentNotificationService environmentNotificationService)
    : IRequestHandler<UploadSftpFileCommand, bool>
{
    public async Task<bool> Handle(UploadSftpFileCommand request, CancellationToken cancellationToken)
    {
        await fileManager.UploadFileAsync(request.Path ?? "", request.FileName, request.FileStream);
        await environmentNotificationService.NotifyEnvironmentActivityAsync(
            "EnvironmentServiceSftp",
            "EnvironmentSftpUploaded",
            request.FileName,
            request.Path ?? "/");
        return true;
    }
}
