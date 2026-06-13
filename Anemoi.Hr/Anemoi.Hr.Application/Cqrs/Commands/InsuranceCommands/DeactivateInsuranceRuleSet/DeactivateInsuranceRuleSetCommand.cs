using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.DeactivateInsuranceRuleSet;

public sealed record DeactivateInsuranceRuleSetCommand(
    string Id,
    string DeactivatedBy = null) : ICommandResult<SuccessResponse>;
