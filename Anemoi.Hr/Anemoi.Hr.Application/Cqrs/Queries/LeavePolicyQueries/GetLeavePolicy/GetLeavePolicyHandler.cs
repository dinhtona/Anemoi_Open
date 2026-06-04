using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Leaves;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.LeavePolicyQueries.GetLeavePolicy;

public sealed class GetLeavePolicyHandler(ISqlRepository<LeavePolicy> repository, LeaveMapper mapper)
    : IQueryHandler<GetLeavePolicyQuery, OneOf<LeavePolicyResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<LeavePolicyResponse, ErrorDetailResponse>> Handle(GetLeavePolicyQuery request,
        CancellationToken cancellationToken)
    {
        var policy = await repository.GetFirstByConditionAsync(x => x.Id == request.Id, null, cancellationToken);
        return policy is null
            ? HrErrorResponses.Create(HrBusinessErrorCodes.LeavePolicyNotFound)
            : mapper.ToLeavePolicyResponse(policy);
    }
}
