using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.MasterData;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.LeavePolicyQueries.GetLeavePolicy;

public sealed class GetLeavePolicyHandler(ISqlRepository<LeavePolicy> repository, MasterDataMapper mapper)
    : IQueryHandler<GetLeavePolicyQuery, OneOf<LeavePolicyResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<LeavePolicyResponse, ErrorDetailResponse>> Handle(GetLeavePolicyQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetFirstByConditionAsync(
            x => x.Id == request.Id,
            q => q.Include(x => x.LeaveType),
            cancellationToken);
        return entity is null
            ? HrErrorResponses.Create(HrBusinessErrorCodes.LeavePolicySettingsNotFound)
            : mapper.ToLeavePolicyResponse(entity);
    }
}
