using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.ActivateInsuranceRuleSet;

public sealed record ActivateInsuranceRuleSetCommand(
    string Id,
    string ActivatedBy = null) : ICommandResult<SuccessResponse>;
