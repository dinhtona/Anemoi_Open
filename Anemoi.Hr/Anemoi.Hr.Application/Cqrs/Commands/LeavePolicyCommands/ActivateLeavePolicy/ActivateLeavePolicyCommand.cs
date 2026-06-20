using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeavePolicyCommands.ActivateLeavePolicy;

public sealed record ActivateLeavePolicyCommand(LeavePolicyId Id) : ICommandResult<SuccessResponse>;
