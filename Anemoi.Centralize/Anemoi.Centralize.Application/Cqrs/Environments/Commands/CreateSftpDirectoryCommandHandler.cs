using System.Threading;
using System.Threading.Tasks;
using Anemoi.Centralize.Application.Abstractions;
using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Commands;

public sealed class CreateSftpDirectoryCommandHandler(ISftpFileManager fileManager)
    : IRequestHandler<CreateSftpDirectoryCommand, bool>
{
    public Task<bool> Handle(CreateSftpDirectoryCommand request, CancellationToken cancellationToken)
    {
        var result = fileManager.CreateDirectory(request.Path);
        return Task.FromResult(result);
    }
}
