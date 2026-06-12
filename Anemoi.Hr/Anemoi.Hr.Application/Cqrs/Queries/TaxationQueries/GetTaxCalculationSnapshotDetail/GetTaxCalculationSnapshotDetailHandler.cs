using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Taxation;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.TaxationQueries.GetTaxCalculationSnapshotDetail;

public sealed class GetTaxCalculationSnapshotDetailHandler(
    ISqlRepository<TaxCalculationSnapshot> repository,
    TaxationMapper mapper)
    : IQueryHandler<GetTaxCalculationSnapshotDetailQuery, OneOf<TaxCalculationSnapshotResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<TaxCalculationSnapshotResponse, ErrorDetailResponse>> Handle(
        GetTaxCalculationSnapshotDetailQuery request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var guid))
        {
            return HrErrorResponses.Create("HR_TAX_CALCULATION_SNAPSHOT_NOT_FOUND");
        }

        var snapshotId = new TaxCalculationSnapshotId(guid);
        var snapshot = await repository.GetFirstByConditionAsync(
            x => x.Id == snapshotId,
            token: cancellationToken);

        if (snapshot is null)
        {
            return HrErrorResponses.Create("HR_TAX_CALCULATION_SNAPSHOT_NOT_FOUND");
        }

        return mapper.ToTaxCalculationSnapshotResponse(snapshot);
    }
}
