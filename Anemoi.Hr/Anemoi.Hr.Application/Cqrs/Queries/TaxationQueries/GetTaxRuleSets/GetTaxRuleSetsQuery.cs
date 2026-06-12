using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.TaxationQueries.GetTaxRuleSets;

public sealed record GetTaxRuleSetsQuery(
    string CountryCode = null,
    string TaxType = null) : IQuery<IReadOnlyCollection<TaxRuleSetResponse>>;
