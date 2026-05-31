using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.Centralize.Application.Abstractions;
using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Queries;

public sealed class DownloadSftpFileQueryHandler(
    ISftpFileManager fileManager,
    IEnvironmentNotificationService environmentNotificationService)
    : IRequestHandler<DownloadSftpFileQuery, DownloadSftpFileResponse>
{
    public async Task<DownloadSftpFileResponse> Handle(
        DownloadSftpFileQuery request,
        CancellationToken cancellationToken)
    {
        var stream = fileManager.DownloadFile(request.Path);
        var fileName = Path.GetFileName(request.Path);
        await environmentNotificationService.NotifyEnvironmentActivityAsync(
            "EnvironmentServiceSftp",
            "EnvironmentSftpDownloaded",
            fileName,
            Path.GetDirectoryName(request.Path) ?? "/");
        return new DownloadSftpFileResponse(stream, fileName, "application/octet-stream");
    }
}
