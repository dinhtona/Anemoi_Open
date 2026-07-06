using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.ActivateTaxRuleSet;

public sealed record ActivateTaxRuleSetCommand(
    string Id,
    string UpdatedBy = null) : ICommandResult<SuccessResponse>;
