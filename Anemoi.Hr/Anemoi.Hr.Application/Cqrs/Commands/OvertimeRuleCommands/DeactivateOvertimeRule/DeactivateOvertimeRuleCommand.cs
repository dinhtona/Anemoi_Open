using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.OvertimeRuleCommands.DeactivateOvertimeRule;

public sealed record DeactivateOvertimeRuleCommand(OvertimeRuleId Id) : ICommandResult<SuccessResponse>;
