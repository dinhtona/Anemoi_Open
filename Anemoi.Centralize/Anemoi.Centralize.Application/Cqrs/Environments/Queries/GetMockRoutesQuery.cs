using System.Collections.Generic;
using Anemoi.Centralize.Application.Abstractions;
using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Queries;

public record GetMockRoutesQuery : IRequest<List<MockRouteDto>>;
