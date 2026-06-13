using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using System;

namespace Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.UpdateInsuranceRuleSet;

public sealed record UpdateInsuranceRuleSetCommand(
    string Id,
    string Name,
    string Currency,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string UpdatedBy = null) : ICommandResult<SuccessResponse>;
