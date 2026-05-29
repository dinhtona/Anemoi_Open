#nullable enable
using System.IO;
using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Commands;

public record UploadSftpFileCommand(string? Path, string FileName, Stream FileStream) : IRequest<bool>;
