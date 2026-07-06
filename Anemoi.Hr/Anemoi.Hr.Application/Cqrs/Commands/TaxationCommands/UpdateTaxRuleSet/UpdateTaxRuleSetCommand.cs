using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using System;

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.UpdateTaxRuleSet;

public sealed record UpdateTaxRuleSetCommand(
    string Id,
    string Name,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string UpdatedBy = null) : ICommandResult<SuccessResponse>;
