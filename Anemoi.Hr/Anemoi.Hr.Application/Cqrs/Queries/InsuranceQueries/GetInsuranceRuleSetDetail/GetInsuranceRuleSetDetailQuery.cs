using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.InsuranceQueries.GetInsuranceRuleSetDetail;

public sealed record GetInsuranceRuleSetDetailQuery(
    string Id) : IQuery<OneOf<InsuranceRuleSetResponse, ErrorDetailResponse>>;
