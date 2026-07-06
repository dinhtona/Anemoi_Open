using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.InsuranceQueries.GetInsuranceRuleSets;

public sealed record GetInsuranceRuleSetsQuery(
    string? CountryCode = null,
    string? InsuranceType = null) : IQuery<IReadOnlyCollection<InsuranceRuleSetResponse>>;
