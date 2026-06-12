using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.TaxationQueries.GetTaxRuleSetDetail;

public sealed record GetTaxRuleSetDetailQuery(
    string Id) : IQuery<OneOf<TaxRuleSetResponse, ErrorDetailResponse>>;
