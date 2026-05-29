using System;
using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Commands;

public record DeleteMockRouteCommand(Guid Id) : IRequest<bool>;
