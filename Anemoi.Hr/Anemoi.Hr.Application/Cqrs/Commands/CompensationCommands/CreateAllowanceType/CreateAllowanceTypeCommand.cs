using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using System;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.CreateAllowanceType;

public sealed record CreateAllowanceTypeCommand(
    string Code,
    string Name,
    string Description,
    bool IsTaxable,
    bool IsActive,
    bool SensitivePermissionConfirmed) : ICommandResult<CreateAllowanceTypeResponse>;
