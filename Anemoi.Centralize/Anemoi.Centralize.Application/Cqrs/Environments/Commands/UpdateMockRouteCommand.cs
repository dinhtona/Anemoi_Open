#nullable enable
using Anemoi.Centralize.Application.Abstractions;
using Anemoi.Centralize.Domain.ModelIds;
using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Commands;

public record UpdateMockRouteCommand(
    MockRouteId Id,
    string Method,
    string Path,
    int StatusCode,
    string ContentType,
    string ResponseBody,
    string Description,
    bool IsActive
) : IRequest<MockRouteDto?>;
