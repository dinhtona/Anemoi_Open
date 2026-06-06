using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.AssignPositionAllowance;

public sealed record AssignPositionAllowanceCommand(
    PositionId PositionId,
    AllowanceTypeId AllowanceTypeId,
    decimal Amount,
    string Currency,
    bool IsActive,
    bool SensitivePermissionConfirmed) : ICommandResult<AssignPositionAllowanceResponse>;
