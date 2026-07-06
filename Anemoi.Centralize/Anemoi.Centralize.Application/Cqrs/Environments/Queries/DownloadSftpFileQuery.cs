using System.IO;
using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Queries;

public sealed record DownloadSftpFileQuery(string Path) : IRequest<DownloadSftpFileResponse>;

public sealed record DownloadSftpFileResponse(Stream Stream, string FileName, string ContentType);
