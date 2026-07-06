using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.InsuranceQueries.GetInsuranceCalculationSnapshotDetail;

public sealed record GetInsuranceCalculationSnapshotDetailQuery(
    string Id) : IQuery<OneOf<InsuranceCalculationSnapshotResponse, ErrorDetailResponse>>;
