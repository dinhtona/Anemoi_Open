#nullable enable
using System;
using Anemoi.Centralize.Application.Abstractions;
using MediatR;

namespace Anemoi.Centralize.Application.Cqrs.Environments.Commands;

public record UpdateMockRouteCommand(
    Guid Id,
    string Method,
    string Path,
    int StatusCode,
    string ContentType,
    string ResponseBody,
    string Description,
    bool IsActive
) : IRequest<MockRouteDto?>;
