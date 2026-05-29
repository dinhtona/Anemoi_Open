using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Commands;

public record StartEnvironmentCommand(string Id) : IRequest<bool>;
