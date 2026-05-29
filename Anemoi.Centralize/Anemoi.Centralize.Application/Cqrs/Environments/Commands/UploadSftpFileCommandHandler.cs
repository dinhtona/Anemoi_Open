using System.Threading;
using System.Threading.Tasks;
using Anemoi.Centralize.Application.Abstractions;
using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Commands;

public sealed class UploadSftpFileCommandHandler(ISftpFileManager fileManager)
    : IRequestHandler<UploadSftpFileCommand, bool>
{
    public async Task<bool> Handle(UploadSftpFileCommand request, CancellationToken cancellationToken)
    {
        await fileManager.UploadFileAsync(request.Path ?? "", request.FileName, request.FileStream);
        return true;
    }
}
