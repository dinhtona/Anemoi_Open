using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.ActivateAllowanceType;

public sealed record ActivateAllowanceTypeCommand(AllowanceTypeId Id) : ICommandResult<SuccessResponse>;
