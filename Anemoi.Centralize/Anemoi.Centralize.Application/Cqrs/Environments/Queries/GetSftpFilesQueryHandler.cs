using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.Centralize.Application.Abstractions;
using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Queries;

public sealed class GetSftpFilesQueryHandler(ISftpFileManager fileManager)
    : IRequestHandler<GetSftpFilesQuery, List<SftpFileDto>>
{
    public Task<List<SftpFileDto>> Handle(GetSftpFilesQuery request, CancellationToken cancellationToken)
    {
        var files = fileManager.ListFiles(request.Path ?? "");
        return Task.FromResult(files);
    }
}
