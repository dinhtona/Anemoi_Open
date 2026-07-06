using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Commands;

public record DeleteSftpFileCommand(string Path) : IRequest<bool>;
