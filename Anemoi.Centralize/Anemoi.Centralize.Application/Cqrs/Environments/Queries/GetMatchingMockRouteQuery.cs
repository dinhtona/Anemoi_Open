#nullable enable
using Anemoi.Centralize.Application.Abstractions;
using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Queries;

public sealed record GetMatchingMockRouteQuery(string Path, string Method) : IRequest<MockRouteDto?>;
