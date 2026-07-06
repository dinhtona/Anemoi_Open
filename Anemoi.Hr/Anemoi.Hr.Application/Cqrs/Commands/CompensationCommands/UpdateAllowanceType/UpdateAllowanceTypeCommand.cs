using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.UpdateAllowanceType;

public sealed record UpdateAllowanceTypeCommand(
    AllowanceTypeId AllowanceTypeId,
    string Name,
    string Description,
    bool IsTaxable,
    bool IsActive,
    bool SensitivePermissionConfirmed) : ICommandResult<UpdateAllowanceTypeResponse>;
