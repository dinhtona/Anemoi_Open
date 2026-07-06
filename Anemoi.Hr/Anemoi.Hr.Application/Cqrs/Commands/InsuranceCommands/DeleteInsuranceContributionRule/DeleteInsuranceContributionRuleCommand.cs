using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.DeleteInsuranceContributionRule;

public sealed record DeleteInsuranceContributionRuleCommand(
    string Id) : ICommandResult<SuccessResponse>;
