using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.DeactivateAllowanceType;

public sealed record DeactivateAllowanceTypeCommand(
    AllowanceTypeId AllowanceTypeId,
    bool SensitivePermissionConfirmed) : ICommandResult<DeactivateAllowanceTypeResponse>;
