using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using System;

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.CreateTaxRuleSet;

public sealed record CreateTaxRuleSetCommand(
    string CountryCode,
    string TaxType,
    string Name,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string CreatedBy = null) : ICommandResult<CreateTaxRuleSetResponse>;
