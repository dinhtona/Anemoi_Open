using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Insurance;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.InsuranceQueries.GetInsuranceCalculationSnapshotDetail;

public sealed class GetInsuranceCalculationSnapshotDetailHandler(
    ISqlRepository<InsuranceCalculationSnapshot> repository,
    InsuranceMapper mapper)
    : IQueryHandler<GetInsuranceCalculationSnapshotDetailQuery, OneOf<InsuranceCalculationSnapshotResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<InsuranceCalculationSnapshotResponse, ErrorDetailResponse>> Handle(
        GetInsuranceCalculationSnapshotDetailQuery request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var guid))
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.InsuranceSnapshotNotFound);
        }

        var snapshotId = new InsuranceCalculationSnapshotId(guid);
        var snapshot = await repository.GetQueryable(x => x.Id == snapshotId)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(cancellationToken);

        if (snapshot is null)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.InsuranceSnapshotNotFound);
        }

        return mapper.ToInsuranceCalculationSnapshotResponse(snapshot);
    }
}
