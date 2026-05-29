using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Commands;

public record CreateSftpDirectoryCommand(string Path) : IRequest<bool>;
