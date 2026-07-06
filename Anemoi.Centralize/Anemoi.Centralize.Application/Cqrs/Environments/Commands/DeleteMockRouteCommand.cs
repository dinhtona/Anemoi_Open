using Anemoi.Centralize.Domain.ModelIds;
using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Commands;

public record DeleteMockRouteCommand(MockRouteId Id) : IRequest<bool>;
