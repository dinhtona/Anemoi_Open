using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using System;

namespace Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.CreateInsuranceRuleSet;

public sealed record CreateInsuranceRuleSetCommand(
    string CountryCode,
    string InsuranceType,
    string Name,
    string Currency,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string CreatedBy = null) : ICommandResult<CreateInsuranceRuleSetResponse>;
