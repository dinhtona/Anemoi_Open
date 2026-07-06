using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.TaxationQueries.GetTaxCalculationSnapshotDetail;

public sealed record GetTaxCalculationSnapshotDetailQuery(
    string Id) : IQuery<OneOf<TaxCalculationSnapshotResponse, ErrorDetailResponse>>;
