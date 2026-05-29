using System.Threading;
using System.Threading.Tasks;
using Anemoi.Centralize.Application.Abstractions;
using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Commands;

public sealed class DeleteSftpFileCommandHandler(ISftpFileManager fileManager)
    : IRequestHandler<DeleteSftpFileCommand, bool>
{
    public Task<bool> Handle(DeleteSftpFileCommand request, CancellationToken cancellationToken)
    {
        var result = fileManager.DeleteFile(request.Path);
        return Task.FromResult(result);
    }
}
