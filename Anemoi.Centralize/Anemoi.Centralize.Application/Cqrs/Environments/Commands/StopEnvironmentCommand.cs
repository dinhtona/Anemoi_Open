using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Commands;

public record StopEnvironmentCommand(string Id) : IRequest<bool>;
