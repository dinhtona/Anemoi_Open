using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.DeactivateTaxRuleSet;

public sealed record DeactivateTaxRuleSetCommand(
    string Id,
    string UpdatedBy = null) : ICommandResult<SuccessResponse>;
